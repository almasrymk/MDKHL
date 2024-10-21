using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParking.Migrations
{
    /// <inheritdoc />
    public partial class enhanceInOrganizationAndInvoiceEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsFullDayExceeded",
                table: "Organizations",
                newName: "IsClose");

            migrationBuilder.RenameColumn(
                name: "FullDay",
                table: "Organizations",
                newName: "FullDayPrice");

            migrationBuilder.RenameColumn(
                name: "ExceededTime",
                table: "Organizations",
                newName: "ClosingHoursStart");

            migrationBuilder.AddColumn<TimeOnly>(
                name: "ClosingHoursEnd",
                table: "Organizations",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClosingHoursEnd",
                table: "Organizations");

            migrationBuilder.RenameColumn(
                name: "IsClose",
                table: "Organizations",
                newName: "IsFullDayExceeded");

            migrationBuilder.RenameColumn(
                name: "FullDayPrice",
                table: "Organizations",
                newName: "FullDay");

            migrationBuilder.RenameColumn(
                name: "ClosingHoursStart",
                table: "Organizations",
                newName: "ExceededTime");
        }
    }
}
