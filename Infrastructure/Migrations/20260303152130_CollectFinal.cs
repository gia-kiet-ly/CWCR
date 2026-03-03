using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CollectFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CollectionRequests_Users_CitizenId",
                table: "CollectionRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_CollectionRequests_WasteReports_ReportId",
                table: "CollectionRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_CollectionRequests_WasteTypes_WasteTypeId",
                table: "CollectionRequests");

            migrationBuilder.DropTable(
                name: "EnterpriseProfiles");

            migrationBuilder.DropIndex(
                name: "IX_EnterpriseWasteCapabilities_EnterpriseId",
                table: "EnterpriseWasteCapabilities");

            migrationBuilder.DropIndex(
                name: "IX_EnterpriseServiceAreas_EnterpriseId",
                table: "EnterpriseServiceAreas");

            migrationBuilder.DropIndex(
                name: "IX_CollectorAssignments_RequestId",
                table: "CollectorAssignments");

            migrationBuilder.DropIndex(
                name: "IX_CollectionRequests_CitizenId",
                table: "CollectionRequests");

            migrationBuilder.DropIndex(
                name: "IX_CollectionRequests_ReportId",
                table: "CollectionRequests");

            migrationBuilder.DropIndex(
                name: "IX_CollectionRequests_WasteTypeId",
                table: "CollectionRequests");

            migrationBuilder.DropColumn(
                name: "CitizenId",
                table: "CollectionRequests");

            migrationBuilder.DropColumn(
                name: "ReportId",
                table: "CollectionRequests");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "RecyclingEnterprises",
                newName: "OperationalStatus");

            migrationBuilder.RenameColumn(
                name: "WasteTypeId",
                table: "CollectionRequests",
                newName: "WasteReportWasteId");

            migrationBuilder.AddColumn<Guid>(
                name: "RecyclingEnterpriseId",
                table: "RecyclingStatistics",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "RepresentativeId",
                table: "RecyclingEnterprises",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "ApprovalStatus",
                table: "RecyclingEnterprises",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "EnvironmentLicenseFileId",
                table: "RecyclingEnterprises",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "LegalRepresentative",
                table: "RecyclingEnterprises",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RepresentativePosition",
                table: "RecyclingEnterprises",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TaxCode",
                table: "RecyclingEnterprises",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "RecyclingEnterprises",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "RecyclingEnterpriseId",
                table: "PointRules",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RegionCode",
                table: "EnterpriseServiceAreas",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<Guid>(
                name: "CollectionRequestId",
                table: "Complaints",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ComplainantId",
                table: "Complaints",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "RecyclingEnterpriseId",
                table: "CollectorProfiles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecyclingStatistics_RecyclingEnterpriseId",
                table: "RecyclingStatistics",
                column: "RecyclingEnterpriseId");

            migrationBuilder.CreateIndex(
                name: "IX_RecyclingEnterprises_UserId",
                table: "RecyclingEnterprises",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PointRules_RecyclingEnterpriseId",
                table: "PointRules",
                column: "RecyclingEnterpriseId");

            migrationBuilder.CreateIndex(
                name: "IX_EnterpriseWasteCapabilities_EnterpriseId_WasteTypeId",
                table: "EnterpriseWasteCapabilities",
                columns: new[] { "EnterpriseId", "WasteTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EnterpriseServiceAreas_EnterpriseId_RegionCode",
                table: "EnterpriseServiceAreas",
                columns: new[] { "EnterpriseId", "RegionCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_CollectionRequestId",
                table: "Complaints",
                column: "CollectionRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_ComplainantId",
                table: "Complaints",
                column: "ComplainantId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectorProfiles_RecyclingEnterpriseId",
                table: "CollectorProfiles",
                column: "RecyclingEnterpriseId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectorAssignments_RequestId",
                table: "CollectorAssignments",
                column: "RequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CollectionRequests_WasteReportWasteId",
                table: "CollectionRequests",
                column: "WasteReportWasteId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CollectionRequests_WasteReportWastes_WasteReportWasteId",
                table: "CollectionRequests",
                column: "WasteReportWasteId",
                principalTable: "WasteReportWastes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CollectorProfiles_RecyclingEnterprises_RecyclingEnterpriseId",
                table: "CollectorProfiles",
                column: "RecyclingEnterpriseId",
                principalTable: "RecyclingEnterprises",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_CollectionRequests_CollectionRequestId",
                table: "Complaints",
                column: "CollectionRequestId",
                principalTable: "CollectionRequests",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_Users_ComplainantId",
                table: "Complaints",
                column: "ComplainantId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PointRules_RecyclingEnterprises_RecyclingEnterpriseId",
                table: "PointRules",
                column: "RecyclingEnterpriseId",
                principalTable: "RecyclingEnterprises",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RecyclingEnterprises_Users_UserId",
                table: "RecyclingEnterprises",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RecyclingStatistics_RecyclingEnterprises_RecyclingEnterpriseId",
                table: "RecyclingStatistics",
                column: "RecyclingEnterpriseId",
                principalTable: "RecyclingEnterprises",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CollectionRequests_WasteReportWastes_WasteReportWasteId",
                table: "CollectionRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_CollectorProfiles_RecyclingEnterprises_RecyclingEnterpriseId",
                table: "CollectorProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_CollectionRequests_CollectionRequestId",
                table: "Complaints");

            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_Users_ComplainantId",
                table: "Complaints");

            migrationBuilder.DropForeignKey(
                name: "FK_PointRules_RecyclingEnterprises_RecyclingEnterpriseId",
                table: "PointRules");

            migrationBuilder.DropForeignKey(
                name: "FK_RecyclingEnterprises_Users_UserId",
                table: "RecyclingEnterprises");

            migrationBuilder.DropForeignKey(
                name: "FK_RecyclingStatistics_RecyclingEnterprises_RecyclingEnterpriseId",
                table: "RecyclingStatistics");

            migrationBuilder.DropIndex(
                name: "IX_RecyclingStatistics_RecyclingEnterpriseId",
                table: "RecyclingStatistics");

            migrationBuilder.DropIndex(
                name: "IX_RecyclingEnterprises_UserId",
                table: "RecyclingEnterprises");

            migrationBuilder.DropIndex(
                name: "IX_PointRules_RecyclingEnterpriseId",
                table: "PointRules");

            migrationBuilder.DropIndex(
                name: "IX_EnterpriseWasteCapabilities_EnterpriseId_WasteTypeId",
                table: "EnterpriseWasteCapabilities");

            migrationBuilder.DropIndex(
                name: "IX_EnterpriseServiceAreas_EnterpriseId_RegionCode",
                table: "EnterpriseServiceAreas");

            migrationBuilder.DropIndex(
                name: "IX_Complaints_CollectionRequestId",
                table: "Complaints");

            migrationBuilder.DropIndex(
                name: "IX_Complaints_ComplainantId",
                table: "Complaints");

            migrationBuilder.DropIndex(
                name: "IX_CollectorProfiles_RecyclingEnterpriseId",
                table: "CollectorProfiles");

            migrationBuilder.DropIndex(
                name: "IX_CollectorAssignments_RequestId",
                table: "CollectorAssignments");

            migrationBuilder.DropIndex(
                name: "IX_CollectionRequests_WasteReportWasteId",
                table: "CollectionRequests");

            migrationBuilder.DropColumn(
                name: "RecyclingEnterpriseId",
                table: "RecyclingStatistics");

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "RecyclingEnterprises");

            migrationBuilder.DropColumn(
                name: "EnvironmentLicenseFileId",
                table: "RecyclingEnterprises");

            migrationBuilder.DropColumn(
                name: "LegalRepresentative",
                table: "RecyclingEnterprises");

            migrationBuilder.DropColumn(
                name: "RepresentativePosition",
                table: "RecyclingEnterprises");

            migrationBuilder.DropColumn(
                name: "TaxCode",
                table: "RecyclingEnterprises");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "RecyclingEnterprises");

            migrationBuilder.DropColumn(
                name: "RecyclingEnterpriseId",
                table: "PointRules");

            migrationBuilder.DropColumn(
                name: "CollectionRequestId",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "ComplainantId",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "RecyclingEnterpriseId",
                table: "CollectorProfiles");

            migrationBuilder.RenameColumn(
                name: "OperationalStatus",
                table: "RecyclingEnterprises",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "WasteReportWasteId",
                table: "CollectionRequests",
                newName: "WasteTypeId");

            migrationBuilder.AlterColumn<Guid>(
                name: "RepresentativeId",
                table: "RecyclingEnterprises",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RegionCode",
                table: "EnterpriseServiceAreas",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<Guid>(
                name: "CitizenId",
                table: "CollectionRequests",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ReportId",
                table: "CollectionRequests",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "EnterpriseProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EnterpriseName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnvironmentLicenseFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LegalRepresentative = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RepresentativePosition = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TaxCode = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnterpriseProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnterpriseProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EnterpriseWasteCapabilities_EnterpriseId",
                table: "EnterpriseWasteCapabilities",
                column: "EnterpriseId");

            migrationBuilder.CreateIndex(
                name: "IX_EnterpriseServiceAreas_EnterpriseId",
                table: "EnterpriseServiceAreas",
                column: "EnterpriseId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectorAssignments_RequestId",
                table: "CollectorAssignments",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionRequests_CitizenId",
                table: "CollectionRequests",
                column: "CitizenId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionRequests_ReportId",
                table: "CollectionRequests",
                column: "ReportId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CollectionRequests_WasteTypeId",
                table: "CollectionRequests",
                column: "WasteTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_EnterpriseProfiles_UserId",
                table: "EnterpriseProfiles",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CollectionRequests_Users_CitizenId",
                table: "CollectionRequests",
                column: "CitizenId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CollectionRequests_WasteReports_ReportId",
                table: "CollectionRequests",
                column: "ReportId",
                principalTable: "WasteReports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CollectionRequests_WasteTypes_WasteTypeId",
                table: "CollectionRequests",
                column: "WasteTypeId",
                principalTable: "WasteTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
