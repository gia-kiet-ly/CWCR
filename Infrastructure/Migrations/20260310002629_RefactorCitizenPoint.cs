using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorCitizenPoint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PointRules_RecyclingEnterprises_EnterpriseId",
                table: "PointRules");

            migrationBuilder.DropIndex(
                name: "IX_PointRules_EnterpriseId_WasteTypeId",
                table: "PointRules");

            migrationBuilder.DropIndex(
                name: "IX_PointRules_WasteTypeId",
                table: "PointRules");

            migrationBuilder.DropColumn(
                name: "EstimatedWeightKg",
                table: "WasteReportWastes");

            migrationBuilder.DropColumn(
                name: "EnterpriseId",
                table: "PointRules");

            migrationBuilder.DropColumn(
                name: "FastCollectionBonus",
                table: "PointRules");

            migrationBuilder.DropColumn(
                name: "QualityMultiplier",
                table: "PointRules");

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "WasteReportWastes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsPointCalculated",
                table: "WasteReports",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PointCalculatedAt",
                table: "WasteReports",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "PointRules",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "CitizenPointHistories",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_WasteReportWaste_Quantity",
                table: "WasteReportWastes",
                sql: "[Quantity] > 0");

            migrationBuilder.CreateIndex(
                name: "IX_PointRules_WasteTypeId",
                table: "PointRules",
                column: "WasteTypeId",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_PointRule_BasePoint",
                table: "PointRules",
                sql: "[BasePoint] > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_WasteReportWaste_Quantity",
                table: "WasteReportWastes");

            migrationBuilder.DropIndex(
                name: "IX_PointRules_WasteTypeId",
                table: "PointRules");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PointRule_BasePoint",
                table: "PointRules");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "WasteReportWastes");

            migrationBuilder.DropColumn(
                name: "IsPointCalculated",
                table: "WasteReports");

            migrationBuilder.DropColumn(
                name: "PointCalculatedAt",
                table: "WasteReports");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "CitizenPointHistories");

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedWeightKg",
                table: "WasteReportWastes",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "PointRules",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EnterpriseId",
                table: "PointRules",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "FastCollectionBonus",
                table: "PointRules",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "QualityMultiplier",
                table: "PointRules",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_PointRules_EnterpriseId_WasteTypeId",
                table: "PointRules",
                columns: new[] { "EnterpriseId", "WasteTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PointRules_WasteTypeId",
                table: "PointRules",
                column: "WasteTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_PointRules_RecyclingEnterprises_EnterpriseId",
                table: "PointRules",
                column: "EnterpriseId",
                principalTable: "RecyclingEnterprises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
