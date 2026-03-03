using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CollectCombo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CollectorProfiles_CollectorId",
                table: "CollectorProfiles");

            migrationBuilder.DropIndex(
                name: "IX_CollectorAssignments_CollectorId",
                table: "CollectorAssignments");

            migrationBuilder.DropIndex(
                name: "IX_CollectionRequests_EnterpriseId",
                table: "CollectionRequests");

            migrationBuilder.DropIndex(
                name: "IX_CollectionRequests_ReportId",
                table: "CollectionRequests");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "CollectorAssignments",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "CollectionRequests",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<Guid>(
                name: "CitizenId",
                table: "CollectionRequests",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "WasteTypeId",
                table: "CollectionRequests",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "PublicId",
                table: "CollectionProofs",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_CollectorProfiles_CollectorId",
                table: "CollectorProfiles",
                column: "CollectorId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CollectorAssignments_CollectorId_Status",
                table: "CollectorAssignments",
                columns: new[] { "CollectorId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionRequests_CitizenId",
                table: "CollectionRequests",
                column: "CitizenId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionRequests_EnterpriseId_Status",
                table: "CollectionRequests",
                columns: new[] { "EnterpriseId", "Status" });

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
                name: "IX_CollectionProofs_PublicId",
                table: "CollectionProofs",
                column: "PublicId");

            migrationBuilder.AddForeignKey(
                name: "FK_CollectionRequests_Users_CitizenId",
                table: "CollectionRequests",
                column: "CitizenId",
                principalTable: "Users",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CollectionRequests_Users_CitizenId",
                table: "CollectionRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_CollectionRequests_WasteTypes_WasteTypeId",
                table: "CollectionRequests");

            migrationBuilder.DropIndex(
                name: "IX_CollectorProfiles_CollectorId",
                table: "CollectorProfiles");

            migrationBuilder.DropIndex(
                name: "IX_CollectorAssignments_CollectorId_Status",
                table: "CollectorAssignments");

            migrationBuilder.DropIndex(
                name: "IX_CollectionRequests_CitizenId",
                table: "CollectionRequests");

            migrationBuilder.DropIndex(
                name: "IX_CollectionRequests_EnterpriseId_Status",
                table: "CollectionRequests");

            migrationBuilder.DropIndex(
                name: "IX_CollectionRequests_ReportId",
                table: "CollectionRequests");

            migrationBuilder.DropIndex(
                name: "IX_CollectionRequests_WasteTypeId",
                table: "CollectionRequests");

            migrationBuilder.DropIndex(
                name: "IX_CollectionProofs_PublicId",
                table: "CollectionProofs");

            migrationBuilder.DropColumn(
                name: "CitizenId",
                table: "CollectionRequests");

            migrationBuilder.DropColumn(
                name: "WasteTypeId",
                table: "CollectionRequests");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "CollectionProofs");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "CollectorAssignments",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "CollectionRequests",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_CollectorProfiles_CollectorId",
                table: "CollectorProfiles",
                column: "CollectorId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectorAssignments_CollectorId",
                table: "CollectorAssignments",
                column: "CollectorId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionRequests_EnterpriseId",
                table: "CollectionRequests",
                column: "EnterpriseId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionRequests_ReportId",
                table: "CollectionRequests",
                column: "ReportId");
        }
    }
}
