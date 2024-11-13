using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParking.Migrations
{
    /// <inheritdoc />
    public partial class checkusertypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AddColumn<int>(
            //    name: "Status",
            //    table: "Vehicles",
            //    type: "integer",
            //    nullable: false,
            //    defaultValue: 0);

            //migrationBuilder.AddColumn<int>(
            //    name: "Status",
            //    table: "VehicleModels",
            //    type: "integer",
            //    nullable: false,
            //    defaultValue: 0);

            //migrationBuilder.AddColumn<int>(
            //    name: "Status",
            //    table: "VehicleBrands",
            //    type: "integer",
            //    nullable: false,
            //    defaultValue: 0);

            //migrationBuilder.AddColumn<int>(
            //    name: "Status",
            //    table: "UserTypes",
            //    type: "integer",
            //    nullable: false,
            //    defaultValue: 0);

            //migrationBuilder.AddColumn<int>(
            //    name: "Status",
            //    table: "SubscriptionTypes",
            //    type: "integer",
            //    nullable: false,
            //    defaultValue: 0);

            //migrationBuilder.AddColumn<int>(
            //    name: "StatusSubscription",
            //    table: "Subscriptions",
            //    type: "integer",
            //    nullable: false,
            //    defaultValue: 0);

            //migrationBuilder.AddColumn<int>(
            //    name: "Status",
            //    table: "Screens",
            //    type: "integer",
            //    nullable: false,
            //    defaultValue: 0);

            //migrationBuilder.AddColumn<int>(
            //    name: "Status",
            //    table: "PlateColors",
            //    type: "integer",
            //    nullable: false,
            //    defaultValue: 0);

            //migrationBuilder.AddColumn<int>(
            //    name: "Status",
            //    table: "PackageCategories",
            //    type: "integer",
            //    nullable: false,
            //    defaultValue: 0);

            //migrationBuilder.AddColumn<int>(
            //    name: "Status",
            //    table: "LicenceTypes",
            //    type: "integer",
            //    nullable: false,
            //    defaultValue: 0);

            //migrationBuilder.AddColumn<string>(
            //    name: "CustomerArName",
            //    table: "Invoices",
            //    type: "text",
            //    nullable: true);

            //migrationBuilder.AddColumn<string>(
            //    name: "CustomerEnName",
            //    table: "Invoices",
            //    type: "text",
            //    nullable: true);

            //migrationBuilder.AddColumn<DateTime>(
            //    name: "EntranceTime",
            //    table: "Invoices",
            //    type: "timestamp without time zone",
            //    nullable: true);

            //migrationBuilder.AddColumn<int>(
            //    name: "Status",
            //    table: "Features",
            //    type: "integer",
            //    nullable: false,
            //    defaultValue: 0);

            //migrationBuilder.AddColumn<int>(
            //    name: "Status",
            //    table: "CustomerPackages",
            //    type: "integer",
            //    nullable: false,
            //    defaultValue: 0);

            //migrationBuilder.AddColumn<int>(
            //    name: "Status",
            //    table: "ContactInfos",
            //    type: "integer",
            //    nullable: false,
            //    defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "VehicleModels");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "VehicleBrands");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "UserTypes");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "SubscriptionTypes");

            migrationBuilder.DropColumn(
                name: "StatusSubscription",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Screens");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "PlateColors");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "PackageCategories");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "LicenceTypes");

            migrationBuilder.DropColumn(
                name: "CustomerArName",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CustomerEnName",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "EntranceTime",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Features");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "CustomerPackages");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ContactInfos");
        }
    }
}
