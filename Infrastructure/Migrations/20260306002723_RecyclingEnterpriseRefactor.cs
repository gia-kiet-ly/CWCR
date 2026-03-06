using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RecyclingEnterpriseRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CollectorProfiles_RecyclingEnterprises_RecyclingEnterpriseId",
                table: "CollectorProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_RecyclingEnterprises_Users_RepresentativeId",
                table: "RecyclingEnterprises");

            migrationBuilder.DropIndex(
                name: "IX_RecyclingEnterprises_RepresentativeId",
                table: "RecyclingEnterprises");

            migrationBuilder.DropIndex(
                name: "IX_CollectorProfiles_RecyclingEnterpriseId",
                table: "CollectorProfiles");

            migrationBuilder.DropColumn(
                name: "RepresentativeId",
                table: "RecyclingEnterprises");

            migrationBuilder.DropColumn(
                name: "RecyclingEnterpriseId",
                table: "CollectorProfiles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RepresentativeId",
                table: "RecyclingEnterprises",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RecyclingEnterpriseId",
                table: "CollectorProfiles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecyclingEnterprises_RepresentativeId",
                table: "RecyclingEnterprises",
                column: "RepresentativeId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectorProfiles_RecyclingEnterpriseId",
                table: "CollectorProfiles",
                column: "RecyclingEnterpriseId");

            migrationBuilder.AddForeignKey(
                name: "FK_CollectorProfiles_RecyclingEnterprises_RecyclingEnterpriseId",
                table: "CollectorProfiles",
                column: "RecyclingEnterpriseId",
                principalTable: "RecyclingEnterprises",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RecyclingEnterprises_Users_RepresentativeId",
                table: "RecyclingEnterprises",
                column: "RepresentativeId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
