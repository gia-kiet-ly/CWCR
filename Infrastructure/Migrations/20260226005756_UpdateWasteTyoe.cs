using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateWasteTyoe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WasteTypes_BusinessCode",
                table: "WasteTypes");

            migrationBuilder.DropColumn(
                name: "BusinessCode",
                table: "WasteTypes");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "WasteTypes",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "WasteTypes",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "WasteTypes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "WasteTypes",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WasteTypeId1",
                table: "WasteReportWastes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WasteTypes_Name",
                table: "WasteTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WasteReportWastes_WasteTypeId1",
                table: "WasteReportWastes",
                column: "WasteTypeId1");

            migrationBuilder.AddForeignKey(
                name: "FK_WasteReportWastes_WasteTypes_WasteTypeId1",
                table: "WasteReportWastes",
                column: "WasteTypeId1",
                principalTable: "WasteTypes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WasteReportWastes_WasteTypes_WasteTypeId1",
                table: "WasteReportWastes");

            migrationBuilder.DropIndex(
                name: "IX_WasteTypes_Name",
                table: "WasteTypes");

            migrationBuilder.DropIndex(
                name: "IX_WasteReportWastes_WasteTypeId1",
                table: "WasteReportWastes");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "WasteTypes");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "WasteTypes");

            migrationBuilder.DropColumn(
                name: "WasteTypeId1",
                table: "WasteReportWastes");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "WasteTypes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "WasteTypes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BusinessCode",
                table: "WasteTypes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_WasteTypes_BusinessCode",
                table: "WasteTypes",
                column: "BusinessCode",
                unique: true);
        }
    }
}
