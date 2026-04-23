using System;
using System.Collections.Generic;
using System.Text;

namespace gnosispay_sync.Data.Transactions
{
    public enum TransactionKind
    {
        Payment,
        Refund,
        Reversal
    }
    public sealed class CardTransaction
    {
        public Guid Id { get; set; }
        public string ThreadId { get; set; } = string.Empty;
        public TransactionKind Kind { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ClearedAt { get; set; }
        public bool IsPending { get; set; }

        public long TransactionAmount { get; set; }
        public string TransactionCurrency { get; set; } = string.Empty;
        public short TransactionDecimals { get; set; }

        public long BillingAmount { get; set; }
        public string BillingCurrency { get; set; } = string.Empty;
        public short BillingDecimals { get; set; }

        // === Montants spécifiques aux reversal/refund ===

        public long? ReversalAmount { get; set; }
        public string? ReversalCurrency { get; set; }

        public long? RefundAmount { get; set; }
        public string? RefundCurrency { get; set; }

        // === Marchand ===
        public string Mcc { get; set; } = string.Empty;
        public string MerchantName { get; set; } = string.Empty;
        public string? MerchantCity { get; set; }
        public string MerchantCountry { get; set; } = string.Empty;  // alpha2

        // === Onchain ===

        public string? OnchainTxHash { get; set; }

        // === Audit ===

        public DateTime FetchedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
