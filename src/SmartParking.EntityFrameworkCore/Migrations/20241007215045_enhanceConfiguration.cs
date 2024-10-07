using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParking.Migrations
{
    /// <inheritdoc />
    public partial class enhanceConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "OtpExpirationDate",
                table: "AbpUsers",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValue: new DateTime(2024, 10, 7, 20, 51, 29, 168, DateTimeKind.Utc).AddTicks(5879));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "OtpExpirationDate",
                table: "AbpUsers",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 10, 7, 20, 51, 29, 168, DateTimeKind.Utc).AddTicks(5879),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);
        }
    }
}
