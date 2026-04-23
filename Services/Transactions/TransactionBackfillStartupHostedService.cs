using gnosispay_sync.Database;
using Microsoft.EntityFrameworkCore;

namespace gnosispay_sync.Services.Transactions
{
    public class TransactionBackfillStartupHostedService : IHostedService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TransactionBackfillStartupHostedService> _logger;

        public TransactionBackfillStartupHostedService(
            IServiceScopeFactory scopeFactory,
            ILogger<TransactionBackfillStartupHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken ct)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await RunBackfillIfNeededAsync(ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Backfill failed");
                }
            }, ct);

            await Task.CompletedTask;
        }

        private async Task RunBackfillIfNeededAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasAny = await db.CardTransactions.AnyAsync(ct);

            if (hasAny)
            {
                _logger.LogInformation("Transactions table already populated, skipping backfill");
                return;
            }

            _logger.LogInformation("Transactions table empty, triggering initial backfill");

            var backfillService = scope.ServiceProvider.GetRequiredService<ITransactionBackfillService>();
            var result = await backfillService.RunAsync(ct);

            _logger.LogInformation(
                "Initial backfill done: {Fetched} fetched, {Upserted} upserted",
                result.TotalFetched, result.Upserted);
        }

        public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
    }
}
