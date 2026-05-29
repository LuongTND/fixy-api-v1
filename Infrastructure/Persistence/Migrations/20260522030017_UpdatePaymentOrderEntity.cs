using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePaymentOrderEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WalletTransactions_ExternalTransactionId",
                table: "WalletTransactions");

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentOrderId",
                table: "WalletTransactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1f7d8c1-3e21-4a8c-9b11-2d7f4c5e1001"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 22, 3, 0, 16, 99, DateTimeKind.Utc).AddTicks(9908));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b2e8c9d2-4f32-4b9d-8c22-3e8f5d6f2002"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 22, 3, 0, 16, 99, DateTimeKind.Utc).AddTicks(9911));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c3f9d0e3-5a43-4cad-9d33-4f9a6e7f3003"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 22, 3, 0, 16, 99, DateTimeKind.Utc).AddTicks(9915));

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_ExternalTransactionId",
                table: "WalletTransactions",
                column: "ExternalTransactionId",
                unique: true,
                filter: "[ExternalTransactionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_PaymentOrderId",
                table: "WalletTransactions",
                column: "PaymentOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_WalletTransactions_PaymentOrders_PaymentOrderId",
                table: "WalletTransactions",
                column: "PaymentOrderId",
                principalTable: "PaymentOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WalletTransactions_PaymentOrders_PaymentOrderId",
                table: "WalletTransactions");

            migrationBuilder.DropIndex(
                name: "IX_WalletTransactions_ExternalTransactionId",
                table: "WalletTransactions");

            migrationBuilder.DropIndex(
                name: "IX_WalletTransactions_PaymentOrderId",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "PaymentOrderId",
                table: "WalletTransactions");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1f7d8c1-3e21-4a8c-9b11-2d7f4c5e1001"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 21, 8, 19, 2, 155, DateTimeKind.Utc).AddTicks(5166));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b2e8c9d2-4f32-4b9d-8c22-3e8f5d6f2002"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 21, 8, 19, 2, 155, DateTimeKind.Utc).AddTicks(5170));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c3f9d0e3-5a43-4cad-9d33-4f9a6e7f3003"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 21, 8, 19, 2, 155, DateTimeKind.Utc).AddTicks(5172));

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_ExternalTransactionId",
                table: "WalletTransactions",
                column: "ExternalTransactionId");
        }
    }
}
