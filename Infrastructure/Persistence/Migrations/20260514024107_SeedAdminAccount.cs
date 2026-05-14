using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdminAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1f7d8c1-3e21-4a8c-9b11-2d7f4c5e1001"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 14, 2, 41, 5, 536, DateTimeKind.Utc).AddTicks(1910));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b2e8c9d2-4f32-4b9d-8c22-3e8f5d6f2002"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 14, 2, 41, 5, 536, DateTimeKind.Utc).AddTicks(1914));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c3f9d0e3-5a43-4cad-9d33-4f9a6e7f3003"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 14, 2, 41, 5, 536, DateTimeKind.Utc).AddTicks(1915));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CitizenIdIssueDate", "CitizenIdIssuePlace", "CitizenIdNumber", "CitizenIdVerifiedAt", "CreatedDate", "DateOfBirth", "DeletedBy", "DeletedDate", "Email", "FullName", "Gender", "IsActive", "IsCitizenIdVerified", "IsDeleted", "IsEmailVerified", "IsPhoneVerified", "OAuthId", "OAuthProvider", "PasswordHash", "Phone", "TotpEnabled", "TotpSecret", "UpdatedDate" },
                values: new object[] { new Guid("f1a7d8c1-3e21-4a8c-9b11-2d7f4c5e1000"), null, null, null, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "admin@fixy.com", "System Admin", null, true, false, false, true, true, null, null, "$2a$12$R9h/lIPzHZluvJ5lnpEzeu6.WWf.6eT7S/H8ZpXU.v7.4GfP8P1e.", "0000000000", false, null, null });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "RoleId", "UserId", "AssignedAt", "AssignedById", "ExpiresAt" },
                values: new object[] { new Guid("a1f7d8c1-3e21-4a8c-9b11-2d7f4c5e1001"), new Guid("f1a7d8c1-3e21-4a8c-9b11-2d7f4c5e1000"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("a1f7d8c1-3e21-4a8c-9b11-2d7f4c5e1001"), new Guid("f1a7d8c1-3e21-4a8c-9b11-2d7f4c5e1000") });

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("f1a7d8c1-3e21-4a8c-9b11-2d7f4c5e1000"));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1f7d8c1-3e21-4a8c-9b11-2d7f4c5e1001"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 11, 8, 28, 17, 353, DateTimeKind.Utc).AddTicks(3337));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b2e8c9d2-4f32-4b9d-8c22-3e8f5d6f2002"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 11, 8, 28, 17, 353, DateTimeKind.Utc).AddTicks(3339));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c3f9d0e3-5a43-4cad-9d33-4f9a6e7f3003"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 11, 8, 28, 17, 353, DateTimeKind.Utc).AddTicks(3341));
        }
    }
}
