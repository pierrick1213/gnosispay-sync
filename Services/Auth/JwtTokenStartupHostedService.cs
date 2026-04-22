using System;
using System.Collections.Generic;
using System.Text;

namespace gnosispay_sync.Services.Auth
{
    public class JwtTokenStartupHostedService : IHostedService
    {
        private readonly IInMemoryJwtTokenProvider _tokenProvider;
        private readonly ILogger<JwtTokenStartupHostedService> _logger;

        public JwtTokenStartupHostedService(
        IInMemoryJwtTokenProvider tokenProvider,
        ILogger<JwtTokenStartupHostedService> logger)
        {
            _tokenProvider = tokenProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Generating initial JWT on startup");
                await _tokenProvider.GetValidTokenAsync(ct);
                _logger.LogInformation("Initial JWT ready");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate initial JWT — will retry on first use");
            }
        }

        public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
    }
}
