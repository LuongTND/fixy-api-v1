using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateWorkerPayoutAccountEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkerPayoutAccounts_WorkerProfiles_WorkerId",
                table: "WorkerPayoutAccounts"
            );

            migrationBuilder.RenameColumn(
                name: "WorkerId",
                table: "WorkerPayoutAccounts",
                newName: "WorkerProfileId"
            );

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1f7d8c1-3e21-4a8c-9b11-2d7f4c5e1001"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 26, 2, 50, 12, 838, DateTimeKind.Utc).AddTicks(3742)
            );

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b2e8c9d2-4f32-4b9d-8c22-3e8f5d6f2002"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 26, 2, 50, 12, 838, DateTimeKind.Utc).AddTicks(3746)
            );

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c3f9d0e3-5a43-4cad-9d33-4f9a6e7f3003"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 26, 2, 50, 12, 838, DateTimeKind.Utc).AddTicks(3748)
            );

            migrationBuilder.AddForeignKey(
                name: "FK_WorkerPayoutAccounts_WorkerProfiles_WorkerProfileId",
                table: "WorkerPayoutAccounts",
                column: "WorkerProfileId",
                principalTable: "WorkerProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkerPayoutAccounts_WorkerProfiles_WorkerProfileId",
                table: "WorkerPayoutAccounts"
            );

            migrationBuilder.RenameColumn(
                name: "WorkerProfileId",
                table: "WorkerPayoutAccounts",
                newName: "WorkerId"
            );

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1f7d8c1-3e21-4a8c-9b11-2d7f4c5e1001"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 22, 5, 11, 49, 793, DateTimeKind.Utc).AddTicks(4216)
            );

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b2e8c9d2-4f32-4b9d-8c22-3e8f5d6f2002"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 22, 5, 11, 49, 793, DateTimeKind.Utc).AddTicks(4220)
            );

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c3f9d0e3-5a43-4cad-9d33-4f9a6e7f3003"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 22, 5, 11, 49, 793, DateTimeKind.Utc).AddTicks(4222)
            );

            migrationBuilder.AddForeignKey(
                name: "FK_WorkerPayoutAccounts_WorkerProfiles_WorkerId",
                table: "WorkerPayoutAccounts",
                column: "WorkerId",
                principalTable: "WorkerProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );
        }
    }
}
