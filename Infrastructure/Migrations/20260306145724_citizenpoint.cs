using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class citizenpoint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CitizenPoints_WasteReports_ReportId",
                table: "CitizenPoints");

            migrationBuilder.DropIndex(
                name: "IX_PointRules_EnterpriseId",
                table: "PointRules");

            migrationBuilder.DropIndex(
                name: "IX_CitizenPoints_CitizenId",
                table: "CitizenPoints");

            migrationBuilder.DropIndex(
                name: "IX_CitizenPoints_ReportId",
                table: "CitizenPoints");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "CitizenPoints");

            migrationBuilder.DropColumn(
                name: "ReportId",
                table: "CitizenPoints");

            migrationBuilder.RenameColumn(
                name: "Point",
                table: "CitizenPoints",
                newName: "TotalPoints");

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedWeightKg",
                table: "WasteReportWastes",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RegionCode",
                table: "WasteReports",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "CitizenPointHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CitizenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WasteReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Points = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CitizenPointId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastUpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DeletedTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CitizenPointHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CitizenPointHistories_CitizenPoints_CitizenPointId",
                        column: x => x.CitizenPointId,
                        principalTable: "CitizenPoints",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CitizenPointHistories_Users_CitizenId",
                        column: x => x.CitizenId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CitizenPointHistories_WasteReports_WasteReportId",
                        column: x => x.WasteReportId,
                        principalTable: "WasteReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WasteReports_RegionCode",
                table: "WasteReports",
                column: "RegionCode");

            migrationBuilder.CreateIndex(
                name: "IX_PointRules_EnterpriseId_WasteTypeId",
                table: "PointRules",
                columns: new[] { "EnterpriseId", "WasteTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CitizenPoints_CitizenId",
                table: "CitizenPoints",
                column: "CitizenId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CitizenPointHistories_CitizenId",
                table: "CitizenPointHistories",
                column: "CitizenId");

            migrationBuilder.CreateIndex(
                name: "IX_CitizenPointHistories_CitizenPointId",
                table: "CitizenPointHistories",
                column: "CitizenPointId");

            migrationBuilder.CreateIndex(
                name: "IX_CitizenPointHistories_CreatedTime",
                table: "CitizenPointHistories",
                column: "CreatedTime");

            migrationBuilder.CreateIndex(
                name: "IX_CitizenPointHistories_WasteReportId",
                table: "CitizenPointHistories",
                column: "WasteReportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CitizenPointHistories");

            migrationBuilder.DropIndex(
                name: "IX_WasteReports_RegionCode",
                table: "WasteReports");

            migrationBuilder.DropIndex(
                name: "IX_PointRules_EnterpriseId_WasteTypeId",
                table: "PointRules");

            migrationBuilder.DropIndex(
                name: "IX_CitizenPoints_CitizenId",
                table: "CitizenPoints");

            migrationBuilder.DropColumn(
                name: "EstimatedWeightKg",
                table: "WasteReportWastes");

            migrationBuilder.RenameColumn(
                name: "TotalPoints",
                table: "CitizenPoints",
                newName: "Point");

            migrationBuilder.AlterColumn<string>(
                name: "RegionCode",
                table: "WasteReports",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "CitizenPoints",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "ReportId",
                table: "CitizenPoints",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_PointRules_EnterpriseId",
                table: "PointRules",
                column: "EnterpriseId");

            migrationBuilder.CreateIndex(
                name: "IX_CitizenPoints_CitizenId",
                table: "CitizenPoints",
                column: "CitizenId");

            migrationBuilder.CreateIndex(
                name: "IX_CitizenPoints_ReportId",
                table: "CitizenPoints",
                column: "ReportId");

            migrationBuilder.AddForeignKey(
                name: "FK_CitizenPoints_WasteReports_ReportId",
                table: "CitizenPoints",
                column: "ReportId",
                principalTable: "WasteReports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
