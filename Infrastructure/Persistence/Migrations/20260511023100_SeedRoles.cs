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
                    { new Guid("a1f7d8c1-3e21-4a8c-9b11-2d7f4c5e1001"), new DateTime(2026, 5, 11, 2, 30, 58, 942, DateTimeKind.Utc).AddTicks(5436), null, true, "ADMIN", null },
                    { new Guid("b2e8c9d2-4f32-4b9d-8c22-3e8f5d6f2002"), new DateTime(2026, 5, 11, 2, 30, 58, 942, DateTimeKind.Utc).AddTicks(5438), null, true, "CUSTOMER", null },
                    { new Guid("c3f9d0e3-5a43-4cad-9d33-4f9a6e7f3003"), new DateTime(2026, 5, 11, 2, 30, 58, 942, DateTimeKind.Utc).AddTicks(5440), null, true, "STAFF", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1f7d8c1-3e21-4a8c-9b11-2d7f4c5e1001"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b2e8c9d2-4f32-4b9d-8c22-3e8f5d6f2002"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c3f9d0e3-5a43-4cad-9d33-4f9a6e7f3003"));
        }
    }
}
