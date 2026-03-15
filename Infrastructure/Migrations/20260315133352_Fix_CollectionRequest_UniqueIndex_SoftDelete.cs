using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Fix_CollectionRequest_UniqueIndex_SoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DisputeResolutions_Users_HandlerId",
                table: "DisputeResolutions");

            migrationBuilder.DropIndex(
                name: "IX_CollectionRequests_WasteReportWasteId_EnterpriseId",
                table: "CollectionRequests");

            migrationBuilder.RenameColumn(
                name: "ResolutionNote",
                table: "DisputeResolutions",
                newName: "ResponseNote");

            migrationBuilder.RenameColumn(
                name: "HandlerId",
                table: "DisputeResolutions",
                newName: "EnterpriseId");

            migrationBuilder.RenameIndex(
                name: "IX_DisputeResolutions_HandlerId",
                table: "DisputeResolutions",
                newName: "IX_DisputeResolutions_EnterpriseId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionRequests_WasteReportWasteId_EnterpriseId",
                table: "CollectionRequests",
                columns: new[] { "WasteReportWasteId", "EnterpriseId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_DisputeResolutions_Users_EnterpriseId",
                table: "DisputeResolutions",
                column: "EnterpriseId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DisputeResolutions_Users_EnterpriseId",
                table: "DisputeResolutions");

            migrationBuilder.DropIndex(
                name: "IX_CollectionRequests_WasteReportWasteId_EnterpriseId",
                table: "CollectionRequests");

            migrationBuilder.RenameColumn(
                name: "ResponseNote",
                table: "DisputeResolutions",
                newName: "ResolutionNote");

            migrationBuilder.RenameColumn(
                name: "EnterpriseId",
                table: "DisputeResolutions",
                newName: "HandlerId");

            migrationBuilder.RenameIndex(
                name: "IX_DisputeResolutions_EnterpriseId",
                table: "DisputeResolutions",
                newName: "IX_DisputeResolutions_HandlerId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionRequests_WasteReportWasteId_EnterpriseId",
                table: "CollectionRequests",
                columns: new[] { "WasteReportWasteId", "EnterpriseId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DisputeResolutions_Users_HandlerId",
                table: "DisputeResolutions",
                column: "HandlerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
