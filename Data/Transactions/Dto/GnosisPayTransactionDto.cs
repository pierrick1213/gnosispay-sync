using System;
using System.Collections.Generic;
using System.Text;

namespace gnosispay_sync.Data.Transactions.Dto
{
    public sealed class GnosisPayTransactionDto
    {
        public string ThreadId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ClearedAt { get; set; }
        public bool IsPending { get; set; }
        public string Kind { get; set; } = string.Empty;
        public string? Status { get; set; }

        public string TransactionAmount { get; set; } = "0";
        public CurrencyDto? TransactionCurrency { get; set; }

        public string BillingAmount { get; set; } = "0";
        public CurrencyDto? BillingCurrency { get; set; }

        public string? ReversalAmount { get; set; }
        public CurrencyDto? ReversalCurrency { get; set; }

        public string? RefundAmount { get; set; }
        public CurrencyDto? RefundCurrency { get; set; }

        public string TransactionType { get; set; } = string.Empty;
        public string Mcc { get; set; } = string.Empty;
        public MerchantDto? Merchant { get; set; }
        public CountryDto? Country { get; set; }
        public string CardToken { get; set; } = string.Empty;
        public bool? ImpactsCashback { get; set; }

        public List<OnchainTxDto>? Transactions { get; set; }
    }

    public sealed class CurrencyDto
    {
        public string Symbol { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public short Decimals { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public sealed class MerchantDto
    {
        public string Name { get; set; } = string.Empty;
        public string? City { get; set; }
        public CountryDto? Country { get; set; }
    }

    public sealed class CountryDto
    {
        public string Name { get; set; } = string.Empty;
        public string Numeric { get; set; } = string.Empty;
        public string Alpha2 { get; set; } = string.Empty;
        public string Alpha3 { get; set; } = string.Empty;
    }

    public sealed class OnchainTxDto
    {
        public string Status { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;
        public string Hash { get; set; } = string.Empty;
    }
}
