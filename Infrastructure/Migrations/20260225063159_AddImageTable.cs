using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddImageTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "WasteReports",
                type: "decimal(10,7)",
                precision: 10,
                scale: 7,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,7)",
                oldPrecision: 10,
                oldScale: 7);

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "WasteReports",
                type: "decimal(10,7)",
                precision: 10,
                scale: 7,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,7)",
                oldPrecision: 10,
                oldScale: 7);

            migrationBuilder.AddColumn<string>(
                name: "PublicId",
                table: "WasteImages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "WasteImages");

            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "WasteReports",
                type: "decimal(10,7)",
                precision: 10,
                scale: 7,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,7)",
                oldPrecision: 10,
                oldScale: 7,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "WasteReports",
                type: "decimal(10,7)",
                precision: 10,
                scale: 7,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,7)",
                oldPrecision: 10,
                oldScale: 7,
                oldNullable: true);
        }
    }
}
