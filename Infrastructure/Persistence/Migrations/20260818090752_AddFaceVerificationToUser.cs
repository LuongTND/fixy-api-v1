using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFaceVerificationToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FaceImageUrl",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "FaceMatchScore",
                table: "Users",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FaceVerifiedAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFaceMatched",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1f7d8c1-3e21-4a8c-9b11-2d7f4c5e1001"),
                column: "CreatedDate",
                value: new DateTime(2026, 8, 18, 9, 7, 50, 192, DateTimeKind.Utc).AddTicks(6226));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b2e8c9d2-4f32-4b9d-8c22-3e8f5d6f2002"),
                column: "CreatedDate",
                value: new DateTime(2026, 8, 18, 9, 7, 50, 192, DateTimeKind.Utc).AddTicks(6230));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c3f9d0e3-5a43-4cad-9d33-4f9a6e7f3003"),
                column: "CreatedDate",
                value: new DateTime(2026, 8, 18, 9, 7, 50, 192, DateTimeKind.Utc).AddTicks(6231));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("f1a7d8c1-3e21-4a8c-9b11-2d7f4c5e1000"),
                columns: new[] { "FaceImageUrl", "FaceMatchScore", "FaceVerifiedAt", "IsFaceMatched" },
                values: new object[] { null, null, null, false });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FaceImageUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FaceMatchScore",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FaceVerifiedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsFaceMatched",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1f7d8c1-3e21-4a8c-9b11-2d7f4c5e1001"),
                column: "CreatedDate",
                value: new DateTime(2026, 8, 10, 14, 33, 50, 34, DateTimeKind.Utc).AddTicks(4150));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b2e8c9d2-4f32-4b9d-8c22-3e8f5d6f2002"),
                column: "CreatedDate",
                value: new DateTime(2026, 8, 10, 14, 33, 50, 34, DateTimeKind.Utc).AddTicks(4154));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c3f9d0e3-5a43-4cad-9d33-4f9a6e7f3003"),
                column: "CreatedDate",
                value: new DateTime(2026, 8, 10, 14, 33, 50, 34, DateTimeKind.Utc).AddTicks(4157));
        }
    }
}
