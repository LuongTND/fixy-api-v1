using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMediaAndWorkerProfileEnity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkerCertificates_WorkerProfiles_WorkerId",
                table: "WorkerCertificates");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkerServices_WorkerProfiles_WorkerId",
                table: "WorkerServices");

            migrationBuilder.DropColumn(
                name: "FileSizeKb",
                table: "Media");

            migrationBuilder.DropColumn(
                name: "HeightPx",
                table: "Media");

            migrationBuilder.DropColumn(
                name: "OriginalName",
                table: "Media");

            migrationBuilder.DropColumn(
                name: "ThumbnailUrl",
                table: "Media");

            migrationBuilder.DropColumn(
                name: "WidthPx",
                table: "Media");

            migrationBuilder.RenameColumn(
                name: "WorkerId",
                table: "WorkerServices",
                newName: "WorkerProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkerServices_WorkerId_CategoryId",
                table: "WorkerServices",
                newName: "IX_WorkerServices_WorkerProfileId_CategoryId");

            migrationBuilder.RenameColumn(
                name: "WorkerId",
                table: "WorkerCertificates",
                newName: "WorkerProfileId");

            migrationBuilder.RenameColumn(
                name: "MimeType",
                table: "Media",
                newName: "FilePublicId");

            migrationBuilder.AddColumn<DateTime>(
                name: "CitizenIdIssueDate",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CitizenIdIssuePlace",
                table: "Users",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CitizenIdNumber",
                table: "Users",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CitizenIdVerifiedAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCitizenIdVerified",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1f7d8c1-3e21-4a8c-9b11-2d7f4c5e1001"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 13, 1, 25, 48, 161, DateTimeKind.Utc).AddTicks(255));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b2e8c9d2-4f32-4b9d-8c22-3e8f5d6f2002"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 13, 1, 25, 48, 161, DateTimeKind.Utc).AddTicks(257));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c3f9d0e3-5a43-4cad-9d33-4f9a6e7f3003"),
                columns: new[] { "CreatedDate", "Name" },
                values: new object[] { new DateTime(2026, 5, 13, 1, 25, 48, 161, DateTimeKind.Utc).AddTicks(277), "WORKER" });

            migrationBuilder.AddForeignKey(
                name: "FK_WorkerCertificates_WorkerProfiles_WorkerProfileId",
                table: "WorkerCertificates",
                column: "WorkerProfileId",
                principalTable: "WorkerProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkerServices_WorkerProfiles_WorkerProfileId",
                table: "WorkerServices",
                column: "WorkerProfileId",
                principalTable: "WorkerProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkerCertificates_WorkerProfiles_WorkerProfileId",
                table: "WorkerCertificates");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkerServices_WorkerProfiles_WorkerProfileId",
                table: "WorkerServices");

            migrationBuilder.DropColumn(
                name: "CitizenIdIssueDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CitizenIdIssuePlace",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CitizenIdNumber",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CitizenIdVerifiedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsCitizenIdVerified",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "WorkerProfileId",
                table: "WorkerServices",
                newName: "WorkerId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkerServices_WorkerProfileId_CategoryId",
                table: "WorkerServices",
                newName: "IX_WorkerServices_WorkerId_CategoryId");

            migrationBuilder.RenameColumn(
                name: "WorkerProfileId",
                table: "WorkerCertificates",
                newName: "WorkerId");

            migrationBuilder.RenameColumn(
                name: "FilePublicId",
                table: "Media",
                newName: "MimeType");

            migrationBuilder.AddColumn<int>(
                name: "FileSizeKb",
                table: "Media",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HeightPx",
                table: "Media",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginalName",
                table: "Media",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailUrl",
                table: "Media",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WidthPx",
                table: "Media",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1f7d8c1-3e21-4a8c-9b11-2d7f4c5e1001"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 12, 1, 57, 0, 735, DateTimeKind.Utc).AddTicks(1065));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b2e8c9d2-4f32-4b9d-8c22-3e8f5d6f2002"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 12, 1, 57, 0, 735, DateTimeKind.Utc).AddTicks(1070));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c3f9d0e3-5a43-4cad-9d33-4f9a6e7f3003"),
                columns: new[] { "CreatedDate", "Name" },
                values: new object[] { new DateTime(2026, 5, 12, 1, 57, 0, 735, DateTimeKind.Utc).AddTicks(1073), "STAFF" });

            migrationBuilder.AddForeignKey(
                name: "FK_WorkerCertificates_WorkerProfiles_WorkerId",
                table: "WorkerCertificates",
                column: "WorkerId",
                principalTable: "WorkerProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkerServices_WorkerProfiles_WorkerId",
                table: "WorkerServices",
                column: "WorkerId",
                principalTable: "WorkerProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
