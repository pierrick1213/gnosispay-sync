using gnosispay_sync.Data.Transactions.Dto;
using gnosispay_sync.Database;
using gnosispay_sync.Database.Mapper;
using Microsoft.EntityFrameworkCore;

namespace gnosispay_sync.Services.Transactions
{
    public interface ITransactionSyncService
    {
        Task<SyncResult> SyncNewAsync(CancellationToken ct = default);
        Task<SyncResult> RefreshPendingAsync(CancellationToken ct = default);

    }
    public sealed record SyncResult(int NewTransactions, int UpdatedTransactions);
    public sealed class TransactionSyncService : ITransactionSyncService
    {
        private const int PageSize = 100;
        private readonly IGnosisPayTransactionsClient _client;
        private readonly AppDbContext _db;
        private readonly ILogger<TransactionSyncService> _logger;

        public TransactionSyncService(
        IGnosisPayTransactionsClient client,
        AppDbContext db,
        ILogger<TransactionSyncService> logger)
        {
            _client = client;
            _db = db;
            _logger = logger;
        }

        public async Task<SyncResult> SyncNewAsync(CancellationToken ct = default)
        {
            // 1. Déterminer la date de la plus récente transaction en DB
            var latestCreatedAt = await _db.CardTransactions
                .MaxAsync(t => (DateTime?)t.CreatedAt, ct);

            if (latestCreatedAt is null)
            {
                _logger.LogWarning("No transactions in DB — sync skipped (run backfill first)");
                return new SyncResult(0, 0);
            }

            _logger.LogInformation(
                "Syncing new transactions since {LastCreated:u}",
                latestCreatedAt.Value);

            // On retire 1 minute pour attraper d'éventuelles tx arrivées
            // quasi en même temps que la dernière (sécurité).
            var afterFilter = latestCreatedAt.Value.AddMinutes(-1);

            var totalNew = 0;
            var totalUpdated = 0;
            var offset = 0;

            // 2. Récupérer les nouvelles transactions
            while (true)
            {
                var page = await _client.GetPageAsync(offset, PageSize, afterFilter, ct);

                if (page.Results.Count == 0)
                    break;

                var (inserted, updated) = await UpsertBatchAsync(page.Results, ct);
                totalNew += inserted;
                totalUpdated += updated;

                if (string.IsNullOrEmpty(page.Next))
                    break;

                offset += PageSize;
            }

            return new SyncResult(totalNew, totalUpdated);
        }

        public async Task<SyncResult> RefreshPendingAsync(CancellationToken ct = default)
        {
            var pendings = await _db.CardTransactions
                .Where(t => t.IsPending)
                .ToListAsync(ct);

            if (pendings.Count == 0)
            {
                _logger.LogInformation("No pending transactions to refresh");
                return new SyncResult(0, 0);
            }

            _logger.LogInformation("Refreshing {Count} pending transactions", pendings.Count);

            var oldestPending = pendings.Min(t => t.CreatedAt);
            var afterFilter = oldestPending.AddMinutes(-5);

            var refreshed = 0;
            var offset = 0;

            while (true)
            {
                var page = await _client.GetPageAsync(offset, PageSize, afterFilter, ct);

                if (page.Results.Count == 0)
                    break;

                foreach (var dto in page.Results)
                {
                    var kind = TransactionMapper.ParseKind(dto.Kind);
                    var existing = pendings.FirstOrDefault(
                        p => p.ThreadId == dto.ThreadId && p.Kind == kind);

                    if (existing is null)
                        continue;

                    var newEntity = TransactionMapper.Map(dto, kind);
                    if (existing.IsPending != newEntity.IsPending
                        || existing.ClearedAt != newEntity.ClearedAt
                        || existing.Status != newEntity.Status)
                    {
                        existing.Status = newEntity.Status;
                        existing.ClearedAt = newEntity.ClearedAt;
                        existing.IsPending = newEntity.IsPending;
                        existing.UpdatedAt = DateTime.UtcNow;
                        refreshed++;
                    }
                }

                if (string.IsNullOrEmpty(page.Next))
                    break;

                offset += PageSize;
            }

            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("RefreshPending done: {Updated} updated", refreshed);
            return new SyncResult(0, refreshed);
        }

        private async Task<(int inserted, int updated)> UpsertBatchAsync(
            IEnumerable<GnosisPayTransactionDto> dtos,
            CancellationToken ct)
        {
            var inserted = 0;
            var updated = 0;

            foreach (var dto in dtos)
            {
                var kind = TransactionMapper.ParseKind(dto.Kind);
                var newEntity = TransactionMapper.Map(dto, kind);

                var existing = await _db.CardTransactions
                    .FirstOrDefaultAsync(
                        t => t.ThreadId == newEntity.ThreadId && t.Kind == newEntity.Kind,
                        ct);

                if (existing is null)
                {
                    _db.CardTransactions.Add(newEntity);
                    inserted++;
                }
                else
                {
                    existing.Status = newEntity.Status;
                    existing.ClearedAt = newEntity.ClearedAt;
                    existing.IsPending = newEntity.IsPending;
                    existing.UpdatedAt = DateTime.UtcNow;
                    updated++;
                }
            }

            await _db.SaveChangesAsync(ct);
            return (inserted, updated);
        }
    }
}
