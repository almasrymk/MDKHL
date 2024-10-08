using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParking.Migrations
{
    /// <inheritdoc />
    public partial class customerregestration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "Vehicles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "ArabicName",
                table: "Customers",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "OtpExpirationDate",
                table: "AbpUsers",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 10, 8, 15, 57, 59, 487, DateTimeKind.Local).AddTicks(4428),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "ArabicName",
                table: "Customers");

            migrationBuilder.AlterColumn<DateTime>(
                name: "OtpExpirationDate",
                table: "AbpUsers",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValue: new DateTime(2024, 10, 8, 15, 57, 59, 487, DateTimeKind.Local).AddTicks(4428));
        }
    }
}
