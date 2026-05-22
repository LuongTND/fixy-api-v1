using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVoucherDecoupledArchitecture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vouchers_ServiceCategories_CategoryId",
                table: "Vouchers");

            migrationBuilder.DropIndex(
                name: "idx_voucher_active",
                table: "Vouchers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Vouchers");

            migrationBuilder.DropColumn(
                name: "MaxUsage",
                table: "Vouchers");

            migrationBuilder.DropColumn(
                name: "UsedCount",
                table: "Vouchers");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "Vouchers",
                newName: "ServiceCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Vouchers_CategoryId",
                table: "Vouchers",
                newName: "IX_Vouchers_ServiceCategoryId");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Vouchers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Vouchers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Vouchers",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "VoucherQuotas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VoucherId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaxUsage = table.Column<int>(type: "int", nullable: true),
                    UsedCount = table.Column<int>(type: "int", nullable: false),
                    MaxUsagePerUser = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoucherQuotas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VoucherQuotas_Vouchers_VoucherId",
                        column: x => x.VoucherId,
                        principalTable: "Vouchers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VoucherRestrictions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VoucherId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoucherRestrictions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VoucherRestrictions_Vouchers_VoucherId",
                        column: x => x.VoucherId,
                        principalTable: "Vouchers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VoucherUsageHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VoucherId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DiscountAmount = table.Column<long>(type: "bigint", nullable: false),
                    AppliedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false),
                    FailReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoucherUsageHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VoucherUsageHistories_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VoucherUsageHistories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VoucherUsageHistories_Vouchers_VoucherId",
                        column: x => x.VoucherId,
                        principalTable: "Vouchers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1f7d8c1-3e21-4a8c-9b11-2d7f4c5e1001"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 22, 8, 15, 38, 943, DateTimeKind.Utc).AddTicks(2693));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b2e8c9d2-4f32-4b9d-8c22-3e8f5d6f2002"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 22, 8, 15, 38, 943, DateTimeKind.Utc).AddTicks(2696));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c3f9d0e3-5a43-4cad-9d33-4f9a6e7f3003"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 22, 8, 15, 38, 943, DateTimeKind.Utc).AddTicks(2697));

            migrationBuilder.CreateIndex(
                name: "idx_voucher_status",
                table: "Vouchers",
                columns: new[] { "Status", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_VoucherQuotas_VoucherId",
                table: "VoucherQuotas",
                column: "VoucherId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_restriction_voucher_type",
                table: "VoucherRestrictions",
                columns: new[] { "VoucherId", "Type" });

            migrationBuilder.CreateIndex(
                name: "idx_usage_booking",
                table: "VoucherUsageHistories",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "idx_usage_voucher_user",
                table: "VoucherUsageHistories",
                columns: new[] { "VoucherId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_VoucherUsageHistories_UserId",
                table: "VoucherUsageHistories",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vouchers_ServiceCategories_ServiceCategoryId",
                table: "Vouchers",
                column: "ServiceCategoryId",
                principalTable: "ServiceCategories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vouchers_ServiceCategories_ServiceCategoryId",
                table: "Vouchers");

            migrationBuilder.DropTable(
                name: "VoucherQuotas");

            migrationBuilder.DropTable(
                name: "VoucherRestrictions");

            migrationBuilder.DropTable(
                name: "VoucherUsageHistories");

            migrationBuilder.DropIndex(
                name: "idx_voucher_status",
                table: "Vouchers");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Vouchers");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Vouchers");

            migrationBuilder.RenameColumn(
                name: "ServiceCategoryId",
                table: "Vouchers",
                newName: "CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Vouchers_ServiceCategoryId",
                table: "Vouchers",
                newName: "IX_Vouchers_CategoryId");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Vouchers",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Vouchers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxUsage",
                table: "Vouchers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsedCount",
                table: "Vouchers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1f7d8c1-3e21-4a8c-9b11-2d7f4c5e1001"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 22, 5, 11, 49, 793, DateTimeKind.Utc).AddTicks(4216));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b2e8c9d2-4f32-4b9d-8c22-3e8f5d6f2002"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 22, 5, 11, 49, 793, DateTimeKind.Utc).AddTicks(4220));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c3f9d0e3-5a43-4cad-9d33-4f9a6e7f3003"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 22, 5, 11, 49, 793, DateTimeKind.Utc).AddTicks(4222));

            migrationBuilder.CreateIndex(
                name: "idx_voucher_active",
                table: "Vouchers",
                columns: new[] { "IsActive", "ExpiresAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_Vouchers_ServiceCategories_CategoryId",
                table: "Vouchers",
                column: "CategoryId",
                principalTable: "ServiceCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
