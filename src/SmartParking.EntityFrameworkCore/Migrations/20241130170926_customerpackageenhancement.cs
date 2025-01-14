using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParking.Migrations
{
    /// <inheritdoc />
    public partial class customerpackageenhancement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Package",
                table: "CustomerPackages",
                type: "nvarchar(max)",
                nullable: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_CustomerId",
                table: "Vehicles",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_Customers_CustomerId",
                table: "Vehicles",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
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
            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_Customers_CustomerId",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_CustomerId",
                table: "Vehicles");
        }
    }
}
