using gnosispay_sync.Services.Transactions;
using Quartz;
using System;
using System.Collections.Generic;
using System.Text;

namespace gnosispay_sync.Jobs
{
    [DisallowConcurrentExecution]
    public sealed class SyncNewTransactionsJob : IJob
    {
        private readonly ITransactionSyncService _syncService;
        private readonly ILogger<SyncNewTransactionsJob> _logger;

        public SyncNewTransactionsJob(
            ITransactionSyncService syncService,
            ILogger<SyncNewTransactionsJob> logger)
        {
            _syncService = syncService;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("SyncNewTransactionsJob starting");

            try
            {
                var result = await _syncService.SyncNewAsync(context.CancellationToken);
                _logger.LogInformation("Job done: +{New}", result.NewTransactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SyncNewTransactionsJob failed");
            }
        }
    }
}
