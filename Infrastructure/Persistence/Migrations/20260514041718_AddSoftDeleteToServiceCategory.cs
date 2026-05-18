using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToServiceCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "ServiceCategories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "ServiceCategories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ServiceCategories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1f7d8c1-3e21-4a8c-9b11-2d7f4c5e1001"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 14, 4, 17, 16, 884, DateTimeKind.Utc).AddTicks(8061));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b2e8c9d2-4f32-4b9d-8c22-3e8f5d6f2002"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 14, 4, 17, 16, 884, DateTimeKind.Utc).AddTicks(8064));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c3f9d0e3-5a43-4cad-9d33-4f9a6e7f3003"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 14, 4, 17, 16, 884, DateTimeKind.Utc).AddTicks(8065));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "ServiceCategories");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "ServiceCategories");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ServiceCategories");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1f7d8c1-3e21-4a8c-9b11-2d7f4c5e1001"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 14, 2, 45, 56, 839, DateTimeKind.Utc).AddTicks(2281));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b2e8c9d2-4f32-4b9d-8c22-3e8f5d6f2002"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 14, 2, 45, 56, 839, DateTimeKind.Utc).AddTicks(2287));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c3f9d0e3-5a43-4cad-9d33-4f9a6e7f3003"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 14, 2, 45, 56, 839, DateTimeKind.Utc).AddTicks(2289));
        }
    }
}
