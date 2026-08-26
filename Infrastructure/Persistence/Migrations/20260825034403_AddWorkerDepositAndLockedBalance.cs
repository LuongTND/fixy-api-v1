using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkerDepositAndLockedBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DepositPaidAt",
                table: "WorkerProfiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DepositRequiredAmount",
                table: "WorkerProfiles",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "IsDepositPaid",
                table: "WorkerProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsOffboardingRequested",
                table: "WorkerProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "OffboardingRequestedAt",
                table: "WorkerProfiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LockedBalanceAfter",
                table: "WalletTransactions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LockedBalanceBefore",
                table: "WalletTransactions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LockedBalance",
                table: "Wallets",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1f7d8c1-3e21-4a8c-9b11-2d7f4c5e1001"),
                column: "CreatedDate",
                value: new DateTime(2026, 8, 25, 3, 44, 0, 256, DateTimeKind.Utc).AddTicks(1462));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b2e8c9d2-4f32-4b9d-8c22-3e8f5d6f2002"),
                column: "CreatedDate",
                value: new DateTime(2026, 8, 25, 3, 44, 0, 256, DateTimeKind.Utc).AddTicks(1466));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c3f9d0e3-5a43-4cad-9d33-4f9a6e7f3003"),
                column: "CreatedDate",
                value: new DateTime(2026, 8, 25, 3, 44, 0, 256, DateTimeKind.Utc).AddTicks(1467));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepositPaidAt",
                table: "WorkerProfiles");

            migrationBuilder.DropColumn(
                name: "DepositRequiredAmount",
                table: "WorkerProfiles");

            migrationBuilder.DropColumn(
                name: "IsDepositPaid",
                table: "WorkerProfiles");

            migrationBuilder.DropColumn(
                name: "IsOffboardingRequested",
                table: "WorkerProfiles");

            migrationBuilder.DropColumn(
                name: "OffboardingRequestedAt",
                table: "WorkerProfiles");

            migrationBuilder.DropColumn(
                name: "LockedBalanceAfter",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "LockedBalanceBefore",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "LockedBalance",
                table: "Wallets");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1f7d8c1-3e21-4a8c-9b11-2d7f4c5e1001"),
                column: "CreatedDate",
                value: new DateTime(2026, 8, 19, 3, 51, 7, 581, DateTimeKind.Utc).AddTicks(1293));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b2e8c9d2-4f32-4b9d-8c22-3e8f5d6f2002"),
                column: "CreatedDate",
                value: new DateTime(2026, 8, 19, 3, 51, 7, 581, DateTimeKind.Utc).AddTicks(1299));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c3f9d0e3-5a43-4cad-9d33-4f9a6e7f3003"),
                column: "CreatedDate",
                value: new DateTime(2026, 8, 19, 3, 51, 7, 581, DateTimeKind.Utc).AddTicks(1301));
        }
    }
}
