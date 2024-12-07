using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParking.Migrations
{
    /// <inheritdoc />
    public partial class customerePackage_invoice_enhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPackage",
                table: "Invoices",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CustomerCategory",
                table: "CustomerPackages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PackageArabicName",
                table: "CustomerPackages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PackageEnglishName",
                table: "CustomerPackages",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPackage",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CustomerCategory",
                table: "CustomerPackages");

            migrationBuilder.DropColumn(
                name: "PackageArabicName",
                table: "CustomerPackages");

            migrationBuilder.DropColumn(
                name: "PackageEnglishName",
                table: "CustomerPackages");
        }
    }
}
