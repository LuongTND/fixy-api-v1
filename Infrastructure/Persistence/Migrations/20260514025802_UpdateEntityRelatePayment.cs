using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEntityRelatePayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // PAYMENT ORDER
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentOrders_Users_CustomerId",
                table: "PaymentOrders"
            );

            migrationBuilder.DropIndex(name: "idx_po_customer", table: "PaymentOrders");

            migrationBuilder.DropIndex(name: "idx_po_gateway", table: "PaymentOrders");

            migrationBuilder.DropIndex(name: "IX_PaymentOrders_BookingId", table: "PaymentOrders");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "PaymentOrders",
                newName: "UserId"
            );

            migrationBuilder.AlterColumn<string>(
                name: "GatewayRef",
                table: "PaymentOrders",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<long>(
                name: "DiscountAmount",
                table: "PaymentOrders",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint"
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "BookingId",
                table: "PaymentOrders",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier"
            );

            migrationBuilder.AddColumn<string>(
                name: "ExternalTransactionId",
                table: "PaymentOrders",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "PaymentOrders",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: ""
            );

            // ROLE SEED
            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1f7d8c1-3e21-4a8c-9b11-2d7f4c5e1001"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 14, 2, 58, 1, 972, DateTimeKind.Utc).AddTicks(305)
            );

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b2e8c9d2-4f32-4b9d-8c22-3e8f5d6f2002"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 14, 2, 58, 1, 972, DateTimeKind.Utc).AddTicks(307)
            );

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c3f9d0e3-5a43-4cad-9d33-4f9a6e7f3003"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 14, 2, 58, 1, 972, DateTimeKind.Utc).AddTicks(308)
            );

            // INDEXES
            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrders_BookingId",
                table: "PaymentOrders",
                column: "BookingId",
                unique: true,
                filter: "[BookingId] IS NOT NULL"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrders_ExternalTransactionId",
                table: "PaymentOrders",
                column: "ExternalTransactionId",
                unique: true,
                filter: "[ExternalTransactionId] IS NOT NULL"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrders_Status",
                table: "PaymentOrders",
                column: "Status"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrders_Type",
                table: "PaymentOrders",
                column: "Type"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrders_UserId",
                table: "PaymentOrders",
                column: "UserId"
            );

            // FK
            migrationBuilder.AddForeignKey(
                name: "FK_PaymentOrders_Users_UserId",
                table: "PaymentOrders",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentOrders_Users_UserId",
                table: "PaymentOrders"
            );

            migrationBuilder.DropIndex(name: "IX_PaymentOrders_BookingId", table: "PaymentOrders");

            migrationBuilder.DropIndex(
                name: "IX_PaymentOrders_ExternalTransactionId",
                table: "PaymentOrders"
            );

            migrationBuilder.DropIndex(name: "IX_PaymentOrders_Status", table: "PaymentOrders");

            migrationBuilder.DropIndex(name: "IX_PaymentOrders_Type", table: "PaymentOrders");

            migrationBuilder.DropIndex(name: "IX_PaymentOrders_UserId", table: "PaymentOrders");

            migrationBuilder.DropColumn(name: "ExternalTransactionId", table: "PaymentOrders");

            migrationBuilder.DropColumn(name: "Type", table: "PaymentOrders");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "PaymentOrders",
                newName: "CustomerId"
            );

            migrationBuilder.AlterColumn<string>(
                name: "GatewayRef",
                table: "PaymentOrders",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true
            );

            migrationBuilder.AlterColumn<long>(
                name: "DiscountAmount",
                table: "PaymentOrders",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldDefaultValue: 0L
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "BookingId",
                table: "PaymentOrders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: Guid.Empty,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true
            );

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1f7d8c1-3e21-4a8c-9b11-2d7f4c5e1001"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 13, 9, 25, 44, 810, DateTimeKind.Utc).AddTicks(910)
            );

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b2e8c9d2-4f32-4b9d-8c22-3e8f5d6f2002"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 13, 9, 25, 44, 810, DateTimeKind.Utc).AddTicks(913)
            );

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c3f9d0e3-5a43-4cad-9d33-4f9a6e7f3003"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 13, 9, 25, 44, 810, DateTimeKind.Utc).AddTicks(916)
            );

            migrationBuilder.CreateIndex(
                name: "idx_po_customer",
                table: "PaymentOrders",
                columns: new[] { "CustomerId", "Status" }
            );

            migrationBuilder.CreateIndex(
                name: "idx_po_gateway",
                table: "PaymentOrders",
                column: "GatewayRef"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrders_BookingId",
                table: "PaymentOrders",
                column: "BookingId",
                unique: true
            );

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentOrders_Users_CustomerId",
                table: "PaymentOrders",
                column: "CustomerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );
        }
    }
}
