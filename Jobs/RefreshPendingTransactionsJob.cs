using gnosispay_sync.Services.Transactions;
using Quartz;
using System;
using System.Collections.Generic;
using System.Text;

namespace gnosispay_sync.Jobs
{
    [DisallowConcurrentExecution]
    public class RefreshPendingTransactionsJob : IJob
    {
        private readonly ITransactionSyncService _syncService;
        private readonly ILogger<RefreshPendingTransactionsJob> _logger;

        public RefreshPendingTransactionsJob(
            ITransactionSyncService syncService,
            ILogger<RefreshPendingTransactionsJob> logger)
        {
            _syncService = syncService;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var result = await _syncService.RefreshPendingAsync(context.CancellationToken);
                _logger.LogInformation("Job done: ~{Updated}", result.UpdatedTransactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RefreshPendingTransactionsJob failed");
            }
        }
    }
}
