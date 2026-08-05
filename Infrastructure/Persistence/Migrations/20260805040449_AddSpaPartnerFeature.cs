using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSpaPartnerFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "SpaPartners",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OpeningHours",
                table: "SpaPartners",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "RatingAvg",
                table: "SpaPartners",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "SpaPartners",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalReviews",
                table: "SpaPartners",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "SpaPartnerGalleries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SpaPartnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Caption = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpaPartnerGalleries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpaPartnerGalleries_SpaPartners_SpaPartnerId",
                        column: x => x.SpaPartnerId,
                        principalTable: "SpaPartners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpaPartnerReviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SpaPartnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpaPartnerReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpaPartnerReviews_CustomerProfiles_CustomerProfileId",
                        column: x => x.CustomerProfileId,
                        principalTable: "CustomerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SpaPartnerReviews_SpaPartners_SpaPartnerId",
                        column: x => x.SpaPartnerId,
                        principalTable: "SpaPartners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpaServiceCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpaServiceCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SpaPartnerServices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SpaPartnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SpaServiceCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Price = table.Column<long>(type: "bigint", nullable: false),
                    DiscountedPrice = table.Column<long>(type: "bigint", nullable: true),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpaPartnerServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpaPartnerServices_SpaPartners_SpaPartnerId",
                        column: x => x.SpaPartnerId,
                        principalTable: "SpaPartners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpaPartnerServices_SpaServiceCategories_SpaServiceCategoryId",
                        column: x => x.SpaServiceCategoryId,
                        principalTable: "SpaServiceCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1f7d8c1-3e21-4a8c-9b11-2d7f4c5e1001"),
                column: "CreatedDate",
                value: new DateTime(2026, 8, 5, 4, 4, 46, 752, DateTimeKind.Utc).AddTicks(2605));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b2e8c9d2-4f32-4b9d-8c22-3e8f5d6f2002"),
                column: "CreatedDate",
                value: new DateTime(2026, 8, 5, 4, 4, 46, 752, DateTimeKind.Utc).AddTicks(2609));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c3f9d0e3-5a43-4cad-9d33-4f9a6e7f3003"),
                column: "CreatedDate",
                value: new DateTime(2026, 8, 5, 4, 4, 46, 752, DateTimeKind.Utc).AddTicks(2611));

            migrationBuilder.CreateIndex(
                name: "IX_SpaPartnerGalleries_SpaPartnerId",
                table: "SpaPartnerGalleries",
                column: "SpaPartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_SpaPartnerReviews_CustomerProfileId",
                table: "SpaPartnerReviews",
                column: "CustomerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_SpaPartnerReviews_SpaPartnerId",
                table: "SpaPartnerReviews",
                column: "SpaPartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_SpaPartnerServices_SpaPartnerId",
                table: "SpaPartnerServices",
                column: "SpaPartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_SpaPartnerServices_SpaServiceCategoryId",
                table: "SpaPartnerServices",
                column: "SpaServiceCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SpaServiceCategories_Code",
                table: "SpaServiceCategories",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpaPartnerGalleries");

            migrationBuilder.DropTable(
                name: "SpaPartnerReviews");

            migrationBuilder.DropTable(
                name: "SpaPartnerServices");

            migrationBuilder.DropTable(
                name: "SpaServiceCategories");

            migrationBuilder.DropColumn(
                name: "City",
                table: "SpaPartners");

            migrationBuilder.DropColumn(
                name: "OpeningHours",
                table: "SpaPartners");

            migrationBuilder.DropColumn(
                name: "RatingAvg",
                table: "SpaPartners");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "SpaPartners");

            migrationBuilder.DropColumn(
                name: "TotalReviews",
                table: "SpaPartners");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1f7d8c1-3e21-4a8c-9b11-2d7f4c5e1001"),
                column: "CreatedDate",
                value: new DateTime(2026, 8, 4, 2, 10, 52, 602, DateTimeKind.Utc).AddTicks(9941));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b2e8c9d2-4f32-4b9d-8c22-3e8f5d6f2002"),
                column: "CreatedDate",
                value: new DateTime(2026, 8, 4, 2, 10, 52, 602, DateTimeKind.Utc).AddTicks(9946));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c3f9d0e3-5a43-4cad-9d33-4f9a6e7f3003"),
                column: "CreatedDate",
                value: new DateTime(2026, 8, 4, 2, 10, 52, 602, DateTimeKind.Utc).AddTicks(9948));
        }
    }
}
