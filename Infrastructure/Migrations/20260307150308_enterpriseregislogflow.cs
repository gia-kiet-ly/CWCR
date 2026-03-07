using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class enterpriseregislogflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "EnvironmentLicenseFileId",
                table: "RecyclingEnterprises",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "ApprovalStatus",
                table: "RecyclingEnterprises",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "RecyclingEnterprises",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "RecyclingEnterprises",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReviewedByUserId",
                table: "RecyclingEnterprises",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedAt",
                table: "RecyclingEnterprises",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EnterpriseDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecyclingEnterpriseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StoredFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastUpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DeletedTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnterpriseDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnterpriseDocuments_RecyclingEnterprises_RecyclingEnterpriseId",
                        column: x => x.RecyclingEnterpriseId,
                        principalTable: "RecyclingEnterprises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecyclingEnterprises_ApprovalStatus",
                table: "RecyclingEnterprises",
                column: "ApprovalStatus");

            migrationBuilder.CreateIndex(
                name: "IX_RecyclingEnterprises_ReviewedByUserId",
                table: "RecyclingEnterprises",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RecyclingEnterprises_TaxCode",
                table: "RecyclingEnterprises",
                column: "TaxCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EnterpriseDocuments_RecyclingEnterpriseId",
                table: "EnterpriseDocuments",
                column: "RecyclingEnterpriseId");

            migrationBuilder.CreateIndex(
                name: "IX_EnterpriseDocuments_RecyclingEnterpriseId_DocumentType",
                table: "EnterpriseDocuments",
                columns: new[] { "RecyclingEnterpriseId", "DocumentType" });

            migrationBuilder.AddForeignKey(
                name: "FK_RecyclingEnterprises_Users_ReviewedByUserId",
                table: "RecyclingEnterprises",
                column: "ReviewedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecyclingEnterprises_Users_ReviewedByUserId",
                table: "RecyclingEnterprises");

            migrationBuilder.DropTable(
                name: "EnterpriseDocuments");

            migrationBuilder.DropIndex(
                name: "IX_RecyclingEnterprises_ApprovalStatus",
                table: "RecyclingEnterprises");

            migrationBuilder.DropIndex(
                name: "IX_RecyclingEnterprises_ReviewedByUserId",
                table: "RecyclingEnterprises");

            migrationBuilder.DropIndex(
                name: "IX_RecyclingEnterprises_TaxCode",
                table: "RecyclingEnterprises");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "RecyclingEnterprises");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "RecyclingEnterprises");

            migrationBuilder.DropColumn(
                name: "ReviewedByUserId",
                table: "RecyclingEnterprises");

            migrationBuilder.DropColumn(
                name: "SubmittedAt",
                table: "RecyclingEnterprises");

            migrationBuilder.AlterColumn<Guid>(
                name: "EnvironmentLicenseFileId",
                table: "RecyclingEnterprises",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ApprovalStatus",
                table: "RecyclingEnterprises",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
