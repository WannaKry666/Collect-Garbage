using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarbageCollection.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAddressAddWasteTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "WasteReports");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "WasteReports");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "WasteReports");

            migrationBuilder.RenameColumn(
                name: "WasteType",
                table: "WasteReports",
                newName: "WasteTypes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WasteTypes",
                table: "WasteReports",
                newName: "WasteType");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "WasteReports",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "WasteReports",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "WasteReports",
                type: "double precision",
                nullable: true);
        }
    }
}
