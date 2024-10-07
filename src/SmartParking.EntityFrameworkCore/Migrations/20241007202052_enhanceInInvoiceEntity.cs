using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParking.Migrations
{
    /// <inheritdoc />
    public partial class enhanceInInvoiceEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Invoicetype",
                table: "Invoices",
                newName: "InvoiceType");

            migrationBuilder.AlterColumn<DateTime>(
                name: "OtpExpirationDate",
                table: "AbpUsers",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 10, 7, 20, 21, 29, 27, DateTimeKind.Utc).AddTicks(6590),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValue: new DateTime(2024, 10, 7, 14, 41, 0, 772, DateTimeKind.Utc).AddTicks(5460));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "InvoiceType",
                table: "Invoices",
                newName: "Invoicetype");

            migrationBuilder.AlterColumn<DateTime>(
                name: "OtpExpirationDate",
                table: "AbpUsers",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 10, 7, 14, 41, 0, 772, DateTimeKind.Utc).AddTicks(5460),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValue: new DateTime(2024, 10, 7, 20, 21, 29, 27, DateTimeKind.Utc).AddTicks(6590));
        }
    }
}
