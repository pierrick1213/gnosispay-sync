using gnosispay_sync.Configuration;
using gnosispay_sync.Data.Auth;
using Microsoft.Extensions.Options;
using Nethereum.Signer;
using Nethereum.Siwe.Core;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace gnosispay_sync.Services.Auth
{
    public interface IGnosisPayAuthService
    {
        Task<AuthResult> AuthenticateAsync(CancellationToken ct = default);
    }

    public sealed class GnosisPayAuthService : IGnosisPayAuthService
    {
        private readonly HttpClient _http;
        private readonly GnosisPayOptions _options;
        private readonly ILogger<GnosisPayAuthService> _logger;

        public GnosisPayAuthService(
            HttpClient http,
            IOptions<GnosisPayOptions> options,
            ILogger<GnosisPayAuthService> logger)
        {
            _http = http;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<AuthResult> AuthenticateAsync(CancellationToken ct = default)
        {
            // 1. Récupérer un nonce frais depuis l'API
            var nonce = await FetchNonceAsync(ct);
            _logger.LogInformation("Nonce received");

            // 2. Dériver l'adresse Ethereum depuis la clé privée
            var key = new EthECKey(_options.PrivateKey);
            var address = key.GetPublicAddress();
            _logger.LogInformation("Signing as {Address}", address);

            // 3. Construire le message SIWE
            var issuedAt = DateTime.UtcNow;
            var expiresAt = issuedAt.AddSeconds(_options.JwtTtlInSeconds);
            var siweMessage = new SiweMessage
            {
                Domain = _options.SiweDomain,
                Address = address,
                Statement = _options.SiweStatement,
                Uri = _options.SiweUri,
                Version = "1",
                ChainId = _options.ChainId.ToString(),
                Nonce = nonce,
                IssuedAt = issuedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            };

            var message = SiweMessageStringBuilder.BuildMessage(siweMessage);
            _logger.LogInformation("SIWE message built");

            // 4. Signer le message (EthereumMessageSigner ajoute le préfixe EIP-191)
            var signer = new EthereumMessageSigner();
            var signature = signer.EncodeUTF8AndSign(message, key);
            _logger.LogInformation("Message signed");

            // 5. Échanger la signature contre un JWT
            var jwt = await SubmitChallengeAsync(message, signature, ct);
            _logger.LogInformation("JWT received, expires at {ExpiresAt:u}", expiresAt);

            return new AuthResult(jwt, expiresAt);
        }

        private async Task<string> FetchNonceAsync(CancellationToken ct)
        {
            using var response = await _http.GetAsync("/api/v1/auth/nonce", ct);
            response.EnsureSuccessStatusCode();

            var raw = await response.Content.ReadAsStringAsync(ct);
            var nonce = raw.Trim().Trim('"');

            if (string.IsNullOrWhiteSpace(nonce))
                throw new InvalidOperationException("Empty nonce from Gnosis Pay");

            return nonce;
        }

        private async Task<string> SubmitChallengeAsync(
            string message,
            string signature,
            CancellationToken ct)
        {
            var request = new ChallengeRequest(message, signature, _options.JwtTtlInSeconds);
            using var response = await _http.PostAsJsonAsync("/api/v1/auth/challenge", request, ct);

            var body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Challenge failed: {Status} - Body: {Body}", response.StatusCode, body);
                response.EnsureSuccessStatusCode();
            }

            var result = await response.Content.ReadFromJsonAsync<ChallengeResponse>(cancellationToken: ct);
            if (result is null || string.IsNullOrEmpty(result.token))
                throw new InvalidOperationException("No JWT in challenge response");

            return result.token;
        }
    }
}
