using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class signupcollector : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CollectorProfiles_Users_CollectorId",
                table: "CollectorProfiles");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "CollectorProfiles",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<bool>(
                name: "IsProfileCompleted",
                table: "CollectorProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_CollectorProfiles_Users_CollectorId",
                table: "CollectorProfiles",
                column: "CollectorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CollectorProfiles_Users_CollectorId",
                table: "CollectorProfiles");

            migrationBuilder.DropColumn(
                name: "IsProfileCompleted",
                table: "CollectorProfiles");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "CollectorProfiles",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CollectorProfiles_Users_CollectorId",
                table: "CollectorProfiles",
                column: "CollectorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
