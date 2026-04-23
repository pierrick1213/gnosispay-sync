using System;
using System.Collections.Generic;
using System.Text;

namespace gnosispay_sync.Data.Transactions.Dto
{
    public sealed class TransactionPageDto
    {
        public int Count { get; set; }
        public string? Next { get; set; }
        public string? Previous { get; set; }
        public List<GnosisPayTransactionDto> Results { get; set; } = new();
    }
}
