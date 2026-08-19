using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEContractTermsToBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AcceptedTerms",
                table: "Bookings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "TermsAcceptedAt",
                table: "Bookings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1f7d8c1-3e21-4a8c-9b11-2d7f4c5e1001"),
                column: "CreatedDate",
                value: new DateTime(2026, 8, 19, 3, 51, 7, 581, DateTimeKind.Utc).AddTicks(1293));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b2e8c9d2-4f32-4b9d-8c22-3e8f5d6f2002"),
                column: "CreatedDate",
                value: new DateTime(2026, 8, 19, 3, 51, 7, 581, DateTimeKind.Utc).AddTicks(1299));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c3f9d0e3-5a43-4cad-9d33-4f9a6e7f3003"),
                column: "CreatedDate",
                value: new DateTime(2026, 8, 19, 3, 51, 7, 581, DateTimeKind.Utc).AddTicks(1301));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcceptedTerms",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "TermsAcceptedAt",
                table: "Bookings");

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
        }
    }
}
