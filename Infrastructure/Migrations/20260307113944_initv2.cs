using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class initv2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EnterpriseServiceAreas_EnterpriseId_RegionCode",
                table: "EnterpriseServiceAreas");

            migrationBuilder.DropColumn(
                name: "RegionCode",
                table: "EnterpriseServiceAreas");

            migrationBuilder.AddColumn<Guid>(
                name: "DistrictId",
                table: "EnterpriseServiceAreas",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "WardId",
                table: "EnterpriseServiceAreas",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EnterpriseServiceAreas_DistrictId",
                table: "EnterpriseServiceAreas",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_EnterpriseServiceAreas_EnterpriseId_DistrictId_WardId",
                table: "EnterpriseServiceAreas",
                columns: new[] { "EnterpriseId", "DistrictId", "WardId" },
                unique: true,
                filter: "[WardId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EnterpriseServiceAreas_WardId",
                table: "EnterpriseServiceAreas",
                column: "WardId");

            migrationBuilder.AddForeignKey(
                name: "FK_EnterpriseServiceAreas_Districts_DistrictId",
                table: "EnterpriseServiceAreas",
                column: "DistrictId",
                principalTable: "Districts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EnterpriseServiceAreas_Wards_WardId",
                table: "EnterpriseServiceAreas",
                column: "WardId",
                principalTable: "Wards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EnterpriseServiceAreas_Districts_DistrictId",
                table: "EnterpriseServiceAreas");

            migrationBuilder.DropForeignKey(
                name: "FK_EnterpriseServiceAreas_Wards_WardId",
                table: "EnterpriseServiceAreas");

            migrationBuilder.DropIndex(
                name: "IX_EnterpriseServiceAreas_DistrictId",
                table: "EnterpriseServiceAreas");

            migrationBuilder.DropIndex(
                name: "IX_EnterpriseServiceAreas_EnterpriseId_DistrictId_WardId",
                table: "EnterpriseServiceAreas");

            migrationBuilder.DropIndex(
                name: "IX_EnterpriseServiceAreas_WardId",
                table: "EnterpriseServiceAreas");

            migrationBuilder.DropColumn(
                name: "DistrictId",
                table: "EnterpriseServiceAreas");

            migrationBuilder.DropColumn(
                name: "WardId",
                table: "EnterpriseServiceAreas");

            migrationBuilder.AddColumn<string>(
                name: "RegionCode",
                table: "EnterpriseServiceAreas",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_EnterpriseServiceAreas_EnterpriseId_RegionCode",
                table: "EnterpriseServiceAreas",
                columns: new[] { "EnterpriseId", "RegionCode" },
                unique: true);
        }
    }
}
