using System;
using System.Collections.Generic;
using System.Text;

namespace gnosispay_sync.Services.Auth
{
    public interface IInMemoryJwtTokenProvider
    {
        Task<string> GetValidTokenAsync(CancellationToken ct = default);
        void Invalidate();
    }
    public sealed class InMemoryJwtTokenProvider : IInMemoryJwtTokenProvider
    {
        private static readonly TimeSpan ExpirationBuffer = TimeSpan.FromMinutes(5);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<InMemoryJwtTokenProvider> _logger;
        private readonly SemaphoreSlim _refreshLock = new(1, 1);

        private string? _cachedJwt;
        private DateTime _cachedExpiresAt;

        public InMemoryJwtTokenProvider(
        IServiceScopeFactory scopeFactory,
        ILogger<InMemoryJwtTokenProvider> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task<string> GetValidTokenAsync(CancellationToken ct = default)
        {
            // Chemin rapide : JWT valide en cache, pas de lock nécessaire
            if (IsCachedTokenValid())
                return _cachedJwt!;

            // Chemin lent : refresh protégé par un sémaphore
            // pour éviter que N appels concurrents déclenchent N flows SIWE.
            await _refreshLock.WaitAsync(ct);
            try
            {
                if (IsCachedTokenValid())
                    return _cachedJwt!;

                _logger.LogInformation("JWT missing or expired, triggering SIWE refresh");
                using var scope = _scopeFactory.CreateScope();
                var authService = scope.ServiceProvider.GetRequiredService<IGnosisPayAuthService>();
                var result = await authService.AuthenticateAsync(ct);

                _cachedJwt = result.Jwt;
                _cachedExpiresAt = result.ExpiresAt;

                _logger.LogInformation("JWT refreshed, valid until {ExpiresAt:u}", _cachedExpiresAt);
                return _cachedJwt;
            }
            finally
            {
                _refreshLock.Release();
            }
        }

        public void Invalidate()
        {
            _logger.LogWarning("JWT explicitly invalidated");
            _cachedJwt = null;
            _cachedExpiresAt = DateTime.MinValue;
        }

        private bool IsCachedTokenValid()
        {
            return !string.IsNullOrEmpty(_cachedJwt)
                && DateTime.UtcNow < _cachedExpiresAt - ExpirationBuffer;
        }
    }
}
