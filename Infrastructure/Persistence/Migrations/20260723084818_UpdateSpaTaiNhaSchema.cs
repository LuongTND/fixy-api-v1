using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSpaTaiNhaSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "WorkerServices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                table: "WorkerServices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "WorkerServices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "BioEn",
                table: "WorkerProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BioKo",
                table: "WorkerProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProfileUpdatedAt",
                table: "WorkerProfiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nationality",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredLanguage",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReferralCode",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

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

            migrationBuilder.AddColumn<int>(
                name: "TotalDurationMinutes",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ReferralRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferrerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferredUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferralCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReferredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRewardClaimed = table.Column<bool>(type: "bit", nullable: false),
                    RewardVoucherId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferralRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReferralRecords_Users_ReferredUserId",
                        column: x => x.ReferredUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReferralRecords_Users_ReferrerUserId",
                        column: x => x.ReferrerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReferralRecords_Vouchers_RewardVoucherId",
                        column: x => x.RewardVoucherId,
                        principalTable: "Vouchers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SpaPartners",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CoverImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Lat = table.Column<double>(type: "float", nullable: true),
                    Lng = table.Column<double>(type: "float", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpaPartners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VipMemberships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tier = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TotalSpent = table.Column<long>(type: "bigint", nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VipMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VipMemberships_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpaPartnerPromotions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SpaPartnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiscountPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    OffPeakStartTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    OffPeakEndTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    StartsAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpaPartnerPromotions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpaPartnerPromotions_SpaPartners_SpaPartnerId",
                        column: x => x.SpaPartnerId,
                        principalTable: "SpaPartners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("f1a7d8c1-3e21-4a8c-9b11-2d7f4c5e1000"),
                columns: new[] { "CountryCode", "Nationality", "PreferredLanguage", "ReferralCode" },
                values: new object[] { null, null, "vi", null });

            migrationBuilder.CreateIndex(
                name: "IX_ReferralRecords_ReferredUserId",
                table: "ReferralRecords",
                column: "ReferredUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralRecords_ReferrerUserId",
                table: "ReferralRecords",
                column: "ReferrerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralRecords_RewardVoucherId",
                table: "ReferralRecords",
                column: "RewardVoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_SpaPartnerPromotions_SpaPartnerId",
                table: "SpaPartnerPromotions",
                column: "SpaPartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_VipMemberships_UserId",
                table: "VipMemberships",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReferralRecords");

            migrationBuilder.DropTable(
                name: "SpaPartnerPromotions");

            migrationBuilder.DropTable(
                name: "VipMemberships");

            migrationBuilder.DropTable(
                name: "SpaPartners");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "WorkerServices");

            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                table: "WorkerServices");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "WorkerServices");

            migrationBuilder.DropColumn(
                name: "BioEn",
                table: "WorkerProfiles");

            migrationBuilder.DropColumn(
                name: "BioKo",
                table: "WorkerProfiles");

            migrationBuilder.DropColumn(
                name: "ProfileUpdatedAt",
                table: "WorkerProfiles");

            migrationBuilder.DropColumn(
                name: "CountryCode",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Nationality",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PreferredLanguage",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ReferralCode",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MaxPrice",
                table: "ServiceCategories");

            migrationBuilder.DropColumn(
                name: "MinPrice",
                table: "ServiceCategories");

            migrationBuilder.DropColumn(
                name: "ReferenceDurationMinutes",
                table: "ServiceCategories");

            migrationBuilder.DropColumn(
                name: "TotalDurationMinutes",
                table: "Bookings");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1f7d8c1-3e21-4a8c-9b11-2d7f4c5e1001"),
                column: "CreatedDate",
                value: new DateTime(2026, 6, 18, 4, 8, 26, 645, DateTimeKind.Utc).AddTicks(3568));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b2e8c9d2-4f32-4b9d-8c22-3e8f5d6f2002"),
                column: "CreatedDate",
                value: new DateTime(2026, 6, 18, 4, 8, 26, 645, DateTimeKind.Utc).AddTicks(3572));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c3f9d0e3-5a43-4cad-9d33-4f9a6e7f3003"),
                column: "CreatedDate",
                value: new DateTime(2026, 6, 18, 4, 8, 26, 645, DateTimeKind.Utc).AddTicks(3573));
        }
    }
}
