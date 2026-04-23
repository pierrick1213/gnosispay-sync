using gnosispay_sync.Data.Transactions.Dto;
using gnosispay_sync.Database;
using gnosispay_sync.Database.Mapper;
using Microsoft.EntityFrameworkCore;
using static gnosispay_sync.Services.Transactions.ITransactionBackfillService;

namespace gnosispay_sync.Services.Transactions
{
    public interface ITransactionBackfillService
    {
        Task<BackfillResult> RunAsync(CancellationToken ct = default);

        public sealed record BackfillResult(int TotalFetched, int Upserted);
    }
    public class TransactionBackfillService : ITransactionBackfillService
    {
        private const int PageSize = 100;

        private readonly IGnosisPayTransactionsClient _client;
        private readonly AppDbContext _db;
        private readonly ILogger<TransactionBackfillService> _logger;

        public TransactionBackfillService(
        IGnosisPayTransactionsClient client,
        AppDbContext db,
        ILogger<TransactionBackfillService> logger)
        {
            _client = client;
            _db = db;
            _logger = logger;
        }

        public async Task<BackfillResult> RunAsync(CancellationToken ct = default)
        {
            _logger.LogInformation("Starting full backfill");

            var offset = 0;
            var totalFetched = 0;
            var totalUpserted = 0;

            while (true)
            {
                var page = await _client.GetPageAsync(offset, PageSize, null, ct);

                if (page.Results.Count == 0)
                {
                    _logger.LogInformation("No more transactions to fetch");
                    break;
                }

                var upserted = await UpsertBatchAsync(page.Results, ct);
                totalFetched += page.Results.Count;
                totalUpserted += upserted;

                _logger.LogInformation(
                    "Progress: {Fetched}/{Total} transactions processed",
                    totalFetched, page.Count);

                // Si pas de page suivante, on arrête
                if (string.IsNullOrEmpty(page.Next))
                    break;

                offset += PageSize;
            }

            _logger.LogInformation(
                "Backfill complete: {Fetched} fetched, {Upserted} upserted",
                totalFetched, totalUpserted);

            return new BackfillResult(totalFetched, totalUpserted);
        }

        private async Task<int> UpsertBatchAsync(
            IEnumerable<GnosisPayTransactionDto> dtos,
            CancellationToken ct)
        {
            var entities = dtos
                .Select(dto => TransactionMapper.Map(dto, TransactionMapper.ParseKind(dto.Kind)))
                .ToList();

            foreach (var entity in entities)
            {
                // Clé primaire composite (ThreadId, Kind)
                var existing = await _db.CardTransactions
                    .FirstOrDefaultAsync(
                        t => t.ThreadId == entity.ThreadId && t.Kind == entity.Kind,
                        ct);

                if (existing is null)
                {
                    _db.CardTransactions.Add(entity);
                }
                else
                {
                    // Update des champs qui peuvent changer
                    existing.Status = entity.Status;
                    existing.ClearedAt = entity.ClearedAt;
                    existing.IsPending = entity.IsPending;
                    existing.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _db.SaveChangesAsync(ct);
            return entities.Count;
        }
    }
}
