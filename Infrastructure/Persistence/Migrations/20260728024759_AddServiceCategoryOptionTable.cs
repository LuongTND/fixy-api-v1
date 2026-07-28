using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceCategoryOptionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxPrice",
                table: "ServiceCategories");

            migrationBuilder.DropColumn(
                name: "MinPrice",
                table: "ServiceCategories");

            migrationBuilder.DropColumn(
                name: "ReferenceDurationMinutes",
                table: "ServiceCategories");

            migrationBuilder.CreateTable(
                name: "ServiceCategoryOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<long>(type: "bigint", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceCategoryOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceCategoryOptions_ServiceCategories_ServiceCategoryId",
                        column: x => x.ServiceCategoryId,
                        principalTable: "ServiceCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1f7d8c1-3e21-4a8c-9b11-2d7f4c5e1001"),
                column: "CreatedDate",
                value: new DateTime(2026, 7, 28, 2, 47, 57, 840, DateTimeKind.Utc).AddTicks(7070));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b2e8c9d2-4f32-4b9d-8c22-3e8f5d6f2002"),
                column: "CreatedDate",
                value: new DateTime(2026, 7, 28, 2, 47, 57, 840, DateTimeKind.Utc).AddTicks(7073));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c3f9d0e3-5a43-4cad-9d33-4f9a6e7f3003"),
                column: "CreatedDate",
                value: new DateTime(2026, 7, 28, 2, 47, 57, 840, DateTimeKind.Utc).AddTicks(7075));

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCategoryOptions_ServiceCategoryId",
                table: "ServiceCategoryOptions",
                column: "ServiceCategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceCategoryOptions");

            migrationBuilder.AddColumn<long>(
                name: "MaxPrice",
                table: "ServiceCategories",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "MinPrice",
                table: "ServiceCategories",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReferenceDurationMinutes",
                table: "ServiceCategories",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1f7d8c1-3e21-4a8c-9b11-2d7f4c5e1001"),
                column: "CreatedDate",
                value: new DateTime(2026, 7, 23, 8, 48, 17, 37, DateTimeKind.Utc).AddTicks(1623));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b2e8c9d2-4f32-4b9d-8c22-3e8f5d6f2002"),
                column: "CreatedDate",
                value: new DateTime(2026, 7, 23, 8, 48, 17, 37, DateTimeKind.Utc).AddTicks(1627));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c3f9d0e3-5a43-4cad-9d33-4f9a6e7f3003"),
                column: "CreatedDate",
                value: new DateTime(2026, 7, 23, 8, 48, 17, 37, DateTimeKind.Utc).AddTicks(1629));
        }
    }
}
