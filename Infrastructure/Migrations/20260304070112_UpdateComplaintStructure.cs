using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateComplaintStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_CollectionRequests_CollectionRequestId",
                table: "Complaints");

            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_Users_CitizenId",
                table: "Complaints");

            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_Users_ComplainantId",
                table: "Complaints");

            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_WasteReports_ReportId",
                table: "Complaints");

            migrationBuilder.DropForeignKey(
                name: "FK_DisputeResolutions_Users_AdminId",
                table: "DisputeResolutions");

            migrationBuilder.DropIndex(
                name: "IX_Complaints_CitizenId",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "CitizenId",
                table: "Complaints");

            migrationBuilder.RenameColumn(
                name: "AdminId",
                table: "DisputeResolutions",
                newName: "HandlerId");

            migrationBuilder.RenameIndex(
                name: "IX_DisputeResolutions_AdminId",
                table: "DisputeResolutions",
                newName: "IX_DisputeResolutions_HandlerId");

            migrationBuilder.AlterColumn<string>(
                name: "ResolutionNote",
                table: "DisputeResolutions",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Complaints",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_CollectionRequests_CollectionRequestId",
                table: "Complaints",
                column: "CollectionRequestId",
                principalTable: "CollectionRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_Users_ComplainantId",
                table: "Complaints",
                column: "ComplainantId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_WasteReports_ReportId",
                table: "Complaints",
                column: "ReportId",
                principalTable: "WasteReports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DisputeResolutions_Users_HandlerId",
                table: "DisputeResolutions",
                column: "HandlerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_CollectionRequests_CollectionRequestId",
                table: "Complaints");

            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_Users_ComplainantId",
                table: "Complaints");

            migrationBuilder.DropForeignKey(
                name: "FK_Complaints_WasteReports_ReportId",
                table: "Complaints");

            migrationBuilder.DropForeignKey(
                name: "FK_DisputeResolutions_Users_HandlerId",
                table: "DisputeResolutions");

            migrationBuilder.RenameColumn(
                name: "HandlerId",
                table: "DisputeResolutions",
                newName: "AdminId");

            migrationBuilder.RenameIndex(
                name: "IX_DisputeResolutions_HandlerId",
                table: "DisputeResolutions",
                newName: "IX_DisputeResolutions_AdminId");

            migrationBuilder.AlterColumn<string>(
                name: "ResolutionNote",
                table: "DisputeResolutions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Complaints",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AddColumn<Guid>(
                name: "CitizenId",
                table: "Complaints",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_CitizenId",
                table: "Complaints",
                column: "CitizenId");

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_CollectionRequests_CollectionRequestId",
                table: "Complaints",
                column: "CollectionRequestId",
                principalTable: "CollectionRequests",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_Users_CitizenId",
                table: "Complaints",
                column: "CitizenId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_Users_ComplainantId",
                table: "Complaints",
                column: "ComplainantId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Complaints_WasteReports_ReportId",
                table: "Complaints",
                column: "ReportId",
                principalTable: "WasteReports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DisputeResolutions_Users_AdminId",
                table: "DisputeResolutions",
                column: "AdminId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
