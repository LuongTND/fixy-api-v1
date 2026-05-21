using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WorkerMatchingEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkerMatchingQueues_WorkerProfiles_WorkerId",
                table: "WorkerMatchingQueues");

            migrationBuilder.RenameColumn(
                name: "WorkerId",
                table: "WorkerMatchingQueues",
                newName: "WorkerProfileId");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1f7d8c1-3e21-4a8c-9b11-2d7f4c5e1001"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 21, 4, 53, 25, 752, DateTimeKind.Utc).AddTicks(6578));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b2e8c9d2-4f32-4b9d-8c22-3e8f5d6f2002"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 21, 4, 53, 25, 752, DateTimeKind.Utc).AddTicks(6580));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c3f9d0e3-5a43-4cad-9d33-4f9a6e7f3003"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 21, 4, 53, 25, 752, DateTimeKind.Utc).AddTicks(6581));

            migrationBuilder.AddForeignKey(
                name: "FK_WorkerMatchingQueues_WorkerProfiles_WorkerProfileId",
                table: "WorkerMatchingQueues",
                column: "WorkerProfileId",
                principalTable: "WorkerProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkerMatchingQueues_WorkerProfiles_WorkerProfileId",
                table: "WorkerMatchingQueues");

            migrationBuilder.RenameColumn(
                name: "WorkerProfileId",
                table: "WorkerMatchingQueues",
                newName: "WorkerId");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1f7d8c1-3e21-4a8c-9b11-2d7f4c5e1001"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 21, 3, 39, 46, 789, DateTimeKind.Utc).AddTicks(4338));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b2e8c9d2-4f32-4b9d-8c22-3e8f5d6f2002"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 21, 3, 39, 46, 789, DateTimeKind.Utc).AddTicks(4341));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c3f9d0e3-5a43-4cad-9d33-4f9a6e7f3003"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 21, 3, 39, 46, 789, DateTimeKind.Utc).AddTicks(4342));

            migrationBuilder.AddForeignKey(
                name: "FK_WorkerMatchingQueues_WorkerProfiles_WorkerId",
                table: "WorkerMatchingQueues",
                column: "WorkerId",
                principalTable: "WorkerProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
