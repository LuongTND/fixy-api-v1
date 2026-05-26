using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePayoutRequestEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PayoutRequests_WorkerProfiles_WorkerId",
                table: "PayoutRequests");

            migrationBuilder.RenameColumn(
                name: "WorkerId",
                table: "PayoutRequests",
                newName: "WorkerProfileId");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1f7d8c1-3e21-4a8c-9b11-2d7f4c5e1001"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 26, 3, 14, 6, 831, DateTimeKind.Utc).AddTicks(3189));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b2e8c9d2-4f32-4b9d-8c22-3e8f5d6f2002"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 26, 3, 14, 6, 831, DateTimeKind.Utc).AddTicks(3197));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c3f9d0e3-5a43-4cad-9d33-4f9a6e7f3003"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 26, 3, 14, 6, 831, DateTimeKind.Utc).AddTicks(3199));

            migrationBuilder.AddForeignKey(
                name: "FK_PayoutRequests_WorkerProfiles_WorkerProfileId",
                table: "PayoutRequests",
                column: "WorkerProfileId",
                principalTable: "WorkerProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PayoutRequests_WorkerProfiles_WorkerProfileId",
                table: "PayoutRequests");

            migrationBuilder.RenameColumn(
                name: "WorkerProfileId",
                table: "PayoutRequests",
                newName: "WorkerId");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1f7d8c1-3e21-4a8c-9b11-2d7f4c5e1001"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 22, 8, 54, 40, 220, DateTimeKind.Utc).AddTicks(9213));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b2e8c9d2-4f32-4b9d-8c22-3e8f5d6f2002"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 22, 8, 54, 40, 220, DateTimeKind.Utc).AddTicks(9217));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c3f9d0e3-5a43-4cad-9d33-4f9a6e7f3003"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 22, 8, 54, 40, 220, DateTimeKind.Utc).AddTicks(9219));

            migrationBuilder.AddForeignKey(
                name: "FK_PayoutRequests_WorkerProfiles_WorkerId",
                table: "PayoutRequests",
                column: "WorkerId",
                principalTable: "WorkerProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
