using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAdminHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("f1a7d8c1-3e21-4a8c-9b11-2d7f4c5e1000"),
                column: "PasswordHash",
                value: "$2a$11$2Sn0EJfhBql3sJvuD/UDeODXtuVKzfkeAO8EWTplBPNMFi/P1Gz/.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1f7d8c1-3e21-4a8c-9b11-2d7f4c5e1001"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 14, 2, 41, 5, 536, DateTimeKind.Utc).AddTicks(1910));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b2e8c9d2-4f32-4b9d-8c22-3e8f5d6f2002"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 14, 2, 41, 5, 536, DateTimeKind.Utc).AddTicks(1914));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c3f9d0e3-5a43-4cad-9d33-4f9a6e7f3003"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 14, 2, 41, 5, 536, DateTimeKind.Utc).AddTicks(1915));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("f1a7d8c1-3e21-4a8c-9b11-2d7f4c5e1000"),
                column: "PasswordHash",
                value: "$2a$12$R9h/lIPzHZluvJ5lnpEzeu6.WWf.6eT7S/H8ZpXU.v7.4GfP8P1e.");
        }
    }
}
