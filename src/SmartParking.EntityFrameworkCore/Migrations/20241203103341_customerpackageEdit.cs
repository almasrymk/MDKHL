using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParking.Migrations
{
    /// <inheritdoc />
    public partial class customerpackageEdit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Graceperiod",
                table: "Organizations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckoutMobile",
                table: "GarageTransactions",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DurationOfPackage",
                table: "CustomerPackages",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<DateTime>(
                name: "ActivationDateOfPackage",
                table: "CustomerPackages",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DectivationDateOfPackage",
                table: "CustomerPackages",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionOfPackage",
                table: "CustomerPackages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PriceOfPackage",
                table: "CustomerPackages",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "PublishFromOfPackage",
                table: "CustomerPackages",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "PublishToOfPackage",
                table: "CustomerPackages",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "publishedOfPackage",
                table: "CustomerPackages",
                type: "boolean",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Graceperiod",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "CheckoutMobile",
                table: "GarageTransactions");

            migrationBuilder.DropColumn(
                name: "ActivationDateOfPackage",
                table: "CustomerPackages");

            migrationBuilder.DropColumn(
                name: "DectivationDateOfPackage",
                table: "CustomerPackages");

            migrationBuilder.DropColumn(
                name: "DescriptionOfPackage",
                table: "CustomerPackages");

            migrationBuilder.DropColumn(
                name: "PriceOfPackage",
                table: "CustomerPackages");

            migrationBuilder.DropColumn(
                name: "PublishFromOfPackage",
                table: "CustomerPackages");

            migrationBuilder.DropColumn(
                name: "PublishToOfPackage",
                table: "CustomerPackages");

            migrationBuilder.DropColumn(
                name: "publishedOfPackage",
                table: "CustomerPackages");

            migrationBuilder.AlterColumn<int>(
                name: "DurationOfPackage",
                table: "CustomerPackages",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
