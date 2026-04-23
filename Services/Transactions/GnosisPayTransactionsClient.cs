using gnosispay_sync.Data.Transactions.Dto;
using gnosispay_sync.Services.Auth;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace gnosispay_sync.Services.Transactions
{
    public interface IGnosisPayTransactionsClient
    {
        Task<TransactionPageDto> GetPageAsync(
        int offset,
        int limit,
        DateTime? after = null,
        CancellationToken ct = default);
    }
    public sealed class GnosisPayTransactionsClient : IGnosisPayTransactionsClient
    {
        private readonly HttpClient _http;
        private readonly IInMemoryJwtTokenProvider _tokenProvider;
        private readonly ILogger<GnosisPayTransactionsClient> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        public GnosisPayTransactionsClient(
        HttpClient http,
        IInMemoryJwtTokenProvider tokenProvider,
        ILogger<GnosisPayTransactionsClient> logger)
        {
            _http = http;
            _tokenProvider = tokenProvider;
            _logger = logger;
        }

        public async Task<TransactionPageDto> GetPageAsync(
        int offset,
        int limit,
        DateTime? after = null,
        CancellationToken ct = default)
        {
            var jwt = await _tokenProvider.GetValidTokenAsync(ct);

            var url = $"/api/v1/cards/transactions?offset={offset}&limit={limit}";
            if (after.HasValue)
            {
                var afterStr = after.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                url += $"&after={Uri.EscapeDataString(afterStr)}";
            }

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

            using var response = await _http.SendAsync(request, ct);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning("JWT rejected (401), invalidating cache");
                _tokenProvider.Invalidate();
                throw new UnauthorizedAccessException("JWT rejected by Gnosis Pay API");
            }

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<TransactionPageDto>(JsonOptions, ct);

            if (result is null)
                throw new InvalidOperationException("Empty response from transactions endpoint");

            _logger.LogInformation(
                "Fetched transactions page: offset={Offset}, returned={Count}, total={Total}",
                offset, result.Results.Count, result.Count);

            return result;
        }
    }
}
