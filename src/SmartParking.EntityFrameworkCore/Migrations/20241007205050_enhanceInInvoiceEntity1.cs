using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParking.Migrations
{
    /// <inheritdoc />
    public partial class enhanceInInvoiceEntity1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "InvoiceId",
                table: "Invoices",
                newName: "InvoiceNumber");

            migrationBuilder.AlterColumn<DateTime>(
                name: "OtpExpirationDate",
                table: "AbpUsers",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 10, 7, 20, 51, 29, 168, DateTimeKind.Utc).AddTicks(5879),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValue: new DateTime(2024, 10, 7, 20, 21, 29, 27, DateTimeKind.Utc).AddTicks(6590));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "InvoiceNumber",
                table: "Invoices",
                newName: "InvoiceId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "OtpExpirationDate",
                table: "AbpUsers",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 10, 7, 20, 21, 29, 27, DateTimeKind.Utc).AddTicks(6590),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValue: new DateTime(2024, 10, 7, 20, 51, 29, 168, DateTimeKind.Utc).AddTicks(5879));
        }
    }
}
