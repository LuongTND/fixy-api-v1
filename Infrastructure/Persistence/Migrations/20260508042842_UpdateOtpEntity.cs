using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOtpEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserOtps_Users_UserId",
                table: "UserOtps");

            migrationBuilder.DropIndex(
                name: "idx_otp_expires",
                table: "UserOtps");

            migrationBuilder.DropIndex(
                name: "idx_otp_lookup",
                table: "UserOtps");

            migrationBuilder.DropColumn(
                name: "AttemptCount",
                table: "UserOtps");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "UserOtps");

            migrationBuilder.DropColumn(
                name: "LockedUntil",
                table: "UserOtps");

            migrationBuilder.DropColumn(
                name: "OtpHash",
                table: "UserOtps");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "UserOtps");

            migrationBuilder.DropColumn(
                name: "UsedAt",
                table: "UserOtps");

            migrationBuilder.RenameColumn(
                name: "ExpiresAt",
                table: "UserOtps",
                newName: "ExpiryDate");

            migrationBuilder.AddColumn<bool>(
                name: "IsEmailVerified",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPhoneVerified",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "UserOtps",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<bool>(
                name: "IsUsed",
                table: "UserOtps",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "UserOtps",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OtpCode",
                table: "UserOtps",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Target",
                table: "UserOtps",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_UserOtps_Target",
                table: "UserOtps",
                column: "Target");

            migrationBuilder.CreateIndex(
                name: "IX_UserOtps_UserId",
                table: "UserOtps",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserOtps_Users_UserId",
                table: "UserOtps",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserOtps_Users_UserId",
                table: "UserOtps");

            migrationBuilder.DropIndex(
                name: "IX_UserOtps_Target",
                table: "UserOtps");

            migrationBuilder.DropIndex(
                name: "IX_UserOtps_UserId",
                table: "UserOtps");

            migrationBuilder.DropColumn(
                name: "IsEmailVerified",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsPhoneVerified",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "UserOtps");

            migrationBuilder.DropColumn(
                name: "OtpCode",
                table: "UserOtps");

            migrationBuilder.DropColumn(
                name: "Target",
                table: "UserOtps");

            migrationBuilder.RenameColumn(
                name: "ExpiryDate",
                table: "UserOtps",
                newName: "ExpiresAt");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "UserOtps",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsUsed",
                table: "UserOtps",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "AttemptCount",
                table: "UserOtps",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "UserOtps",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockedUntil",
                table: "UserOtps",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtpHash",
                table: "UserOtps",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "UserOtps",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UsedAt",
                table: "UserOtps",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_otp_expires",
                table: "UserOtps",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "idx_otp_lookup",
                table: "UserOtps",
                columns: new[] { "UserId", "Type", "IsUsed" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserOtps_Users_UserId",
                table: "UserOtps",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
