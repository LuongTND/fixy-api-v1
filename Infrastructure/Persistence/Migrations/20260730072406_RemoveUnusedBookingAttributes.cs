using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedBookingAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "WorkerProposedNote",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "WorkerProposedPrice",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "WorkerProposedTime",
                table: "Bookings");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1f7d8c1-3e21-4a8c-9b11-2d7f4c5e1001"),
                column: "CreatedDate",
                value: new DateTime(2026, 7, 30, 7, 24, 2, 976, DateTimeKind.Utc).AddTicks(7525));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b2e8c9d2-4f32-4b9d-8c22-3e8f5d6f2002"),
                column: "CreatedDate",
                value: new DateTime(2026, 7, 30, 7, 24, 2, 976, DateTimeKind.Utc).AddTicks(7533));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c3f9d0e3-5a43-4cad-9d33-4f9a6e7f3003"),
                column: "CreatedDate",
                value: new DateTime(2026, 7, 30, 7, 24, 2, 976, DateTimeKind.Utc).AddTicks(7534));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Bookings",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WorkerProposedNote",
                table: "Bookings",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "WorkerProposedPrice",
                table: "Bookings",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WorkerProposedTime",
                table: "Bookings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1f7d8c1-3e21-4a8c-9b11-2d7f4c5e1001"),
                column: "CreatedDate",
                value: new DateTime(2026, 7, 28, 8, 5, 39, 512, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b2e8c9d2-4f32-4b9d-8c22-3e8f5d6f2002"),
                column: "CreatedDate",
                value: new DateTime(2026, 7, 28, 8, 5, 39, 512, DateTimeKind.Utc).AddTicks(9525));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c3f9d0e3-5a43-4cad-9d33-4f9a6e7f3003"),
                column: "CreatedDate",
                value: new DateTime(2026, 7, 28, 8, 5, 39, 512, DateTimeKind.Utc).AddTicks(9529));
        }
    }
}
