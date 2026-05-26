using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveServiceCategoryShadowFromVoucher : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vouchers_ServiceCategories_ServiceCategoryId",
                table: "Vouchers");

            migrationBuilder.DropIndex(
                name: "IX_Vouchers_ServiceCategoryId",
                table: "Vouchers");

            migrationBuilder.DropColumn(
                name: "ServiceCategoryId",
                table: "Vouchers");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1f7d8c1-3e21-4a8c-9b11-2d7f4c5e1001"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 26, 2, 37, 46, 205, DateTimeKind.Utc).AddTicks(3812));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b2e8c9d2-4f32-4b9d-8c22-3e8f5d6f2002"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 26, 2, 37, 46, 205, DateTimeKind.Utc).AddTicks(3815));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c3f9d0e3-5a43-4cad-9d33-4f9a6e7f3003"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 26, 2, 37, 46, 205, DateTimeKind.Utc).AddTicks(3817));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ServiceCategoryId",
                table: "Vouchers",
                type: "uniqueidentifier",
                nullable: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_Vouchers_ServiceCategoryId",
                table: "Vouchers",
                column: "ServiceCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vouchers_ServiceCategories_ServiceCategoryId",
                table: "Vouchers",
                column: "ServiceCategoryId",
                principalTable: "ServiceCategories",
                principalColumn: "Id");
        }
    }
}
