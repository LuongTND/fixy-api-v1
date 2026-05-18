using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserAvatar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true
            );

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1f7d8c1-3e21-4a8c-9b11-2d7f4c5e1001"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 18, 9, 41, 16, 313, DateTimeKind.Utc).AddTicks(3352)
            );

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b2e8c9d2-4f32-4b9d-8c22-3e8f5d6f2002"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 18, 9, 41, 16, 313, DateTimeKind.Utc).AddTicks(3357)
            );

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c3f9d0e3-5a43-4cad-9d33-4f9a6e7f3003"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 18, 9, 41, 16, 313, DateTimeKind.Utc).AddTicks(3359)
            );

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("f1a7d8c1-3e21-4a8c-9b11-2d7f4c5e1000"),
                column: "AvatarUrl",
                value: null
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "AvatarUrl", table: "Users");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1f7d8c1-3e21-4a8c-9b11-2d7f4c5e1001"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 15, 7, 49, 3, 443, DateTimeKind.Utc).AddTicks(6139)
            );

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b2e8c9d2-4f32-4b9d-8c22-3e8f5d6f2002"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 15, 7, 49, 3, 443, DateTimeKind.Utc).AddTicks(6142)
            );

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c3f9d0e3-5a43-4cad-9d33-4f9a6e7f3003"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 15, 7, 49, 3, 443, DateTimeKind.Utc).AddTicks(6145)
            );
        }
    }
}
