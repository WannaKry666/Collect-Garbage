using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarbageCollection.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddCitizenTimestamps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_email_otp",
                table: "email_otp");

            migrationBuilder.DropColumn(
                name: "count",
                table: "email_otp");

            migrationBuilder.RenameTable(
                name: "email_otp",
                newName: "email_otps");

            migrationBuilder.RenameIndex(
                name: "IX_email_otp_email",
                table: "email_otps",
                newName: "IX_email_otps_email");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Citizens",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Citizens",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_email_otps",
                table: "email_otps",
                column: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_email_otps",
                table: "email_otps");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Citizens");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Citizens");

            migrationBuilder.RenameTable(
                name: "email_otps",
                newName: "email_otp");

            migrationBuilder.RenameIndex(
                name: "IX_email_otps_email",
                table: "email_otp",
                newName: "IX_email_otp_email");

            migrationBuilder.AddColumn<int>(
                name: "count",
                table: "email_otp",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_email_otp",
                table: "email_otp",
                column: "id");
        }
    }
}
