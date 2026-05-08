using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CreatedDate", "Description", "IsActive", "Name", "UpdatedDate" },
                values: new object[,]
                {
                    { new Guid("6bb3f0d4-0c2e-4a2e-9f5e-30f6f5f6a0b2"), new DateTime(2026, 5, 8, 0, 0, 0, 0, DateTimeKind.Utc), "Worker role", true, "Worker", null },
                    { new Guid("a0a2e2f1-3b6a-4f39-9f9d-1d2a1d2c3b4c"), new DateTime(2026, 5, 8, 0, 0, 0, 0, DateTimeKind.Utc), "Administrator role", true, "Admin", null },
                    { new Guid("b1f5f9b2-2e2c-4ed8-8c1b-6b4d2e8df3a1"), new DateTime(2026, 5, 8, 0, 0, 0, 0, DateTimeKind.Utc), "Customer role", true, "Customer", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("6bb3f0d4-0c2e-4a2e-9f5e-30f6f5f6a0b2"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a0a2e2f1-3b6a-4f39-9f9d-1d2a1d2c3b4c"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b1f5f9b2-2e2c-4ed8-8c1b-6b4d2e8df3a1"));
        }
    }
}
