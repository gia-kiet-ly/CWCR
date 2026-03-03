using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CollectionProof : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReviewNote",
                table: "CollectionProofs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewStatus",
                table: "CollectionProofs",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReviewedAt",
                table: "CollectionProofs",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReviewedBy",
                table: "CollectionProofs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CollectionProofs_ReviewStatus",
                table: "CollectionProofs",
                column: "ReviewStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CollectionProofs_ReviewStatus",
                table: "CollectionProofs");

            migrationBuilder.DropColumn(
                name: "ReviewNote",
                table: "CollectionProofs");

            migrationBuilder.DropColumn(
                name: "ReviewStatus",
                table: "CollectionProofs");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "CollectionProofs");

            migrationBuilder.DropColumn(
                name: "ReviewedBy",
                table: "CollectionProofs");
        }
    }
}
