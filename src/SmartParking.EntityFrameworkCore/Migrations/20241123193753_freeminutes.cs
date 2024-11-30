using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParking.Migrations
{
    /// <inheritdoc />
    public partial class freeminutes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PlateNumber",
                table: "Vehicles",
                newName: "EnPlateNumber");

            migrationBuilder.AddColumn<string>(
                name: "ArPlateNumber",
                table: "Vehicles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsIn",
                table: "UnLinkedTransactions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "DurationOfPackage",
                table: "Packages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FreeMinutes",
                table: "Organizations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsIn",
                table: "GarageTransactions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiryDate",
                table: "CustomerPackages",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArPlateNumber",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "IsIn",
                table: "UnLinkedTransactions");

            migrationBuilder.DropColumn(
                name: "DurationOfPackage",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "FreeMinutes",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "IsIn",
                table: "GarageTransactions");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "CustomerPackages");

            migrationBuilder.RenameColumn(
                name: "EnPlateNumber",
                table: "Vehicles",
                newName: "PlateNumber");
        }
    }
}
