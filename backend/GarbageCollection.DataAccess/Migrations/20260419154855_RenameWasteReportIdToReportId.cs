using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarbageCollection.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RenameWasteReportIdToReportId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "WasteReports",
                newName: "ReportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReportId",
                table: "WasteReports",
                newName: "Id");
        }
    }
}
