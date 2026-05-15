using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateWorkerScheduleEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkerSchedules");

            migrationBuilder.DropTable(
                name: "WorkerServiceAreas");

            migrationBuilder.DropIndex(
                name: "idx_worker_geo",
                table: "WorkerProfiles");

            migrationBuilder.DropColumn(
                name: "Lat",
                table: "WorkerProfiles");

            migrationBuilder.DropColumn(
                name: "Lng",
                table: "WorkerProfiles");

            migrationBuilder.AddColumn<bool>(
                name: "IsAcceptingJobs",
                table: "WorkerProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsBusy",
                table: "WorkerProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "WorkerScheduleExceptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    IsDayOff = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkerScheduleExceptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkerScheduleExceptions_WorkerProfiles_WorkerProfileId",
                        column: x => x.WorkerProfileId,
                        principalTable: "WorkerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkerWeeklySchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkerWeeklySchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkerWeeklySchedules_WorkerProfiles_WorkerProfileId",
                        column: x => x.WorkerProfileId,
                        principalTable: "WorkerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1f7d8c1-3e21-4a8c-9b11-2d7f4c5e1001"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 15, 7, 49, 3, 443, DateTimeKind.Utc).AddTicks(6139));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b2e8c9d2-4f32-4b9d-8c22-3e8f5d6f2002"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 15, 7, 49, 3, 443, DateTimeKind.Utc).AddTicks(6142));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c3f9d0e3-5a43-4cad-9d33-4f9a6e7f3003"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 15, 7, 49, 3, 443, DateTimeKind.Utc).AddTicks(6145));

            migrationBuilder.CreateIndex(
                name: "IX_WorkerScheduleExceptions_Date",
                table: "WorkerScheduleExceptions",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_WorkerScheduleExceptions_WorkerProfileId",
                table: "WorkerScheduleExceptions",
                column: "WorkerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkerScheduleExceptions_WorkerProfileId_Date",
                table: "WorkerScheduleExceptions",
                columns: new[] { "WorkerProfileId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkerWeeklySchedules_WorkerProfileId",
                table: "WorkerWeeklySchedules",
                column: "WorkerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkerWeeklySchedules_WorkerProfileId_DayOfWeek",
                table: "WorkerWeeklySchedules",
                columns: new[] { "WorkerProfileId", "DayOfWeek" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkerScheduleExceptions");

            migrationBuilder.DropTable(
                name: "WorkerWeeklySchedules");

            migrationBuilder.DropColumn(
                name: "IsAcceptingJobs",
                table: "WorkerProfiles");

            migrationBuilder.DropColumn(
                name: "IsBusy",
                table: "WorkerProfiles");

            migrationBuilder.AddColumn<double>(
                name: "Lat",
                table: "WorkerProfiles",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Lng",
                table: "WorkerProfiles",
                type: "float",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WorkerSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BlockedDates = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DayOfWeek = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkerSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkerSchedules_WorkerProfiles_WorkerId",
                        column: x => x.WorkerId,
                        principalTable: "WorkerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkerServiceAreas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    District = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Province = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkerServiceAreas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkerServiceAreas_WorkerProfiles_WorkerId",
                        column: x => x.WorkerId,
                        principalTable: "WorkerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1f7d8c1-3e21-4a8c-9b11-2d7f4c5e1001"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 15, 1, 47, 1, 196, DateTimeKind.Utc).AddTicks(7684));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b2e8c9d2-4f32-4b9d-8c22-3e8f5d6f2002"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 15, 1, 47, 1, 196, DateTimeKind.Utc).AddTicks(7687));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("c3f9d0e3-5a43-4cad-9d33-4f9a6e7f3003"),
                column: "CreatedDate",
                value: new DateTime(2026, 5, 15, 1, 47, 1, 196, DateTimeKind.Utc).AddTicks(7689));

            migrationBuilder.CreateIndex(
                name: "idx_worker_geo",
                table: "WorkerProfiles",
                columns: new[] { "Lat", "Lng" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkerSchedules_WorkerId_DayOfWeek",
                table: "WorkerSchedules",
                columns: new[] { "WorkerId", "DayOfWeek" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkerServiceAreas_WorkerId_Province_District",
                table: "WorkerServiceAreas",
                columns: new[] { "WorkerId", "Province", "District" },
                unique: true);
        }
    }
}
