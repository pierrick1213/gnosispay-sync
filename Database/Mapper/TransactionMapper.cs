using gnosispay_sync.Data.Transactions;
using gnosispay_sync.Data.Transactions.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace gnosispay_sync.Database.Mapper
{
    internal static class TransactionMapper
    {
        public static CardTransaction Map(GnosisPayTransactionDto dto, TransactionKind kind)
        {
            return new CardTransaction
            {
                Id = Guid.NewGuid(),
                ThreadId = dto.ThreadId,
                Kind = kind,
                Status = dto.Status,
                CreatedAt = DateTime.SpecifyKind(dto.CreatedAt, DateTimeKind.Utc),
                ClearedAt = dto.ClearedAt.HasValue
                    ? DateTime.SpecifyKind(dto.ClearedAt.Value, DateTimeKind.Utc)
                    : null,
                IsPending = dto.IsPending,

                TransactionAmount = long.Parse(dto.TransactionAmount),
                TransactionCurrency = dto.TransactionCurrency?.Symbol ?? string.Empty,
                TransactionDecimals = dto.TransactionCurrency?.Decimals ?? 0,

                BillingAmount = long.Parse(dto.BillingAmount),
                BillingCurrency = dto.BillingCurrency?.Symbol ?? string.Empty,
                BillingDecimals = dto.BillingCurrency?.Decimals ?? 0,

                ReversalAmount = dto.ReversalAmount is null ? null : long.Parse(dto.ReversalAmount),
                ReversalCurrency = dto.ReversalCurrency?.Symbol,

                RefundAmount = dto.RefundAmount is null ? null : long.Parse(dto.RefundAmount),
                RefundCurrency = dto.RefundCurrency?.Symbol,

                Mcc = dto.Mcc,
                MerchantName = dto.Merchant?.Name ?? string.Empty,
                MerchantCity = dto.Merchant?.City,
                MerchantCountry = dto.Merchant?.Country?.Alpha2 ?? string.Empty,

                OnchainTxHash = dto.Transactions?.FirstOrDefault()?.Hash,

                FetchedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static TransactionKind ParseKind(string kind) => kind switch
        {
            "Payment" => TransactionKind.Payment,
            "Refund" => TransactionKind.Refund,
            "Reversal" => TransactionKind.Reversal,
            _ => throw new ArgumentException($"Unknown transaction kind: {kind}")
        };
    }
}
