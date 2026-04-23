using gnosispay_sync.Data.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace gnosispay_sync.Database
{
    public sealed class CardTransactionConfiguration : IEntityTypeConfiguration<CardTransaction>
    {
        public void Configure(EntityTypeBuilder<CardTransaction> builder)
        {
            // === Clé primaire composite ===
            builder.HasKey(t => t.Id);

            builder.HasIndex(t => new { t.ThreadId, t.Kind }).IsUnique();

            builder.Property(t => t.ThreadId)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(t => t.Kind)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            // === Statut et dates ===
            builder.Property(t => t.Status).HasMaxLength(50);

            // === Montants ===
            builder.Property(t => t.TransactionCurrency).HasMaxLength(10).IsRequired();
            builder.Property(t => t.BillingCurrency).HasMaxLength(10).IsRequired();
            builder.Property(t => t.ReversalCurrency).HasMaxLength(10);
            builder.Property(t => t.RefundCurrency).HasMaxLength(10);

            // === Marchand ===
            builder.Property(t => t.Mcc).HasMaxLength(10).IsRequired();
            builder.Property(t => t.MerchantName).HasMaxLength(200).IsRequired();
            builder.Property(t => t.MerchantCity).HasMaxLength(200);
            builder.Property(t => t.MerchantCountry).HasMaxLength(5).IsRequired();

            // === Onchain ===
            builder.Property(t => t.OnchainTxHash).HasMaxLength(100);

            // === Index pour les requêtes fréquentes ===
            builder.HasIndex(t => t.CreatedAt);
            builder.HasIndex(t => new { t.IsPending, t.ClearedAt });
            builder.HasIndex(t => t.MerchantCountry);
            builder.HasIndex(t => t.Mcc);
        }
    }
}
