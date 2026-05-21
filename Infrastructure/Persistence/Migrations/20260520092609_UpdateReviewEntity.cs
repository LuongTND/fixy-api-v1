using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateReviewEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_CustomerProfiles_CustomerId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_WorkerProfiles_WorkerId",
                table: "Reviews");

            migrationBuilder.RenameColumn(
                name: "WorkerId",
                table: "Reviews",
                newName: "WorkerProfileId");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "Reviews",
                newName: "CustomerProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_Reviews_CustomerId",
                table: "Reviews",
                newName: "IX_Reviews_CustomerProfileId");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1f7d8c1-3e21-4a8c-9b11-2d7f4c5e1001"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 20, 9, 26, 7, 224, DateTimeKind.Utc).AddTicks(3694));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b2e8c9d2-4f32-4b9d-8c22-3e8f5d6f2002"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 20, 9, 26, 7, 224, DateTimeKind.Utc).AddTicks(3699));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c3f9d0e3-5a43-4cad-9d33-4f9a6e7f3003"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 20, 9, 26, 7, 224, DateTimeKind.Utc).AddTicks(3701));

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_CustomerProfiles_CustomerProfileId",
                table: "Reviews",
                column: "CustomerProfileId",
                principalTable: "CustomerProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_WorkerProfiles_WorkerProfileId",
                table: "Reviews",
                column: "WorkerProfileId",
                principalTable: "WorkerProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_CustomerProfiles_CustomerProfileId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_WorkerProfiles_WorkerProfileId",
                table: "Reviews");

            migrationBuilder.RenameColumn(
                name: "WorkerProfileId",
                table: "Reviews",
                newName: "WorkerId");

            migrationBuilder.RenameColumn(
                name: "CustomerProfileId",
                table: "Reviews",
                newName: "CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_Reviews_CustomerProfileId",
                table: "Reviews",
                newName: "IX_Reviews_CustomerId");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1f7d8c1-3e21-4a8c-9b11-2d7f4c5e1001"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 19, 2, 0, 54, 73, DateTimeKind.Utc).AddTicks(6647));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b2e8c9d2-4f32-4b9d-8c22-3e8f5d6f2002"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 19, 2, 0, 54, 73, DateTimeKind.Utc).AddTicks(6650));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c3f9d0e3-5a43-4cad-9d33-4f9a6e7f3003"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 19, 2, 0, 54, 73, DateTimeKind.Utc).AddTicks(6718));

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_CustomerProfiles_CustomerId",
                table: "Reviews",
                column: "CustomerId",
                principalTable: "CustomerProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_WorkerProfiles_WorkerId",
                table: "Reviews",
                column: "WorkerId",
                principalTable: "WorkerProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
