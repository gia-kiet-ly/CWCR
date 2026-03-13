using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixCollectionRequestIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CollectionRequests_WasteReportWasteId",
                table: "CollectionRequests");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionRequests_WasteReportWasteId_EnterpriseId",
                table: "CollectionRequests",
                columns: new[] { "WasteReportWasteId", "EnterpriseId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CollectionRequests_WasteReportWasteId_EnterpriseId",
                table: "CollectionRequests");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionRequests_WasteReportWasteId",
                table: "CollectionRequests",
                column: "WasteReportWasteId",
                unique: true);
        }
    }
}
