using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDashboardEnterprise : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AssignedTodayKg",
                table: "EnterpriseWasteCapabilities",
                newName: "AssignedTodayCount");

            migrationBuilder.AddColumn<DateOnly>(
                name: "LastResetDate",
                table: "EnterpriseWasteCapabilities",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastResetDate",
                table: "EnterpriseWasteCapabilities");

            migrationBuilder.RenameColumn(
                name: "AssignedTodayCount",
                table: "EnterpriseWasteCapabilities",
                newName: "AssignedTodayKg");
        }
    }
}
