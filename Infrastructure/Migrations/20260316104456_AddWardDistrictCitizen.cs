using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWardDistrictCitizen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DistrictId",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WardId",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_DistrictId",
                table: "Users",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_WardId",
                table: "Users",
                column: "WardId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Districts_DistrictId",
                table: "Users",
                column: "DistrictId",
                principalTable: "Districts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Wards_WardId",
                table: "Users",
                column: "WardId",
                principalTable: "Wards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Districts_DistrictId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Wards_WardId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_DistrictId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_WardId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DistrictId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "WardId",
                table: "Users");
        }
    }
}
