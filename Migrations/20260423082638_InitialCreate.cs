using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace gnosispay_sync.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "card_transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    thread_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    cleared_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_pending = table.Column<bool>(type: "boolean", nullable: false),
                    transaction_amount = table.Column<long>(type: "bigint", nullable: false),
                    transaction_currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    transaction_decimals = table.Column<short>(type: "smallint", nullable: false),
                    billing_amount = table.Column<long>(type: "bigint", nullable: false),
                    billing_currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    billing_decimals = table.Column<short>(type: "smallint", nullable: false),
                    reversal_amount = table.Column<long>(type: "bigint", nullable: true),
                    reversal_currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    refund_amount = table.Column<long>(type: "bigint", nullable: true),
                    refund_currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    mcc = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    merchant_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    merchant_city = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    merchant_country = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    onchain_tx_hash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    fetched_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_card_transactions", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_card_transactions_created_at",
                table: "card_transactions",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_card_transactions_is_pending_cleared_at",
                table: "card_transactions",
                columns: new[] { "is_pending", "cleared_at" });

            migrationBuilder.CreateIndex(
                name: "ix_card_transactions_mcc",
                table: "card_transactions",
                column: "mcc");

            migrationBuilder.CreateIndex(
                name: "ix_card_transactions_merchant_country",
                table: "card_transactions",
                column: "merchant_country");

            migrationBuilder.CreateIndex(
                name: "ix_card_transactions_thread_id_kind",
                table: "card_transactions",
                columns: new[] { "thread_id", "kind" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "card_transactions");
        }
    }
}
