using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParking.Migrations
{
    /// <inheritdoc />
    public partial class customerpackagepayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CustomerPackageId",
                table: "Payments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Discount",
                table: "CustomerPackages",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Vat",
                table: "CustomerPackages",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CustomerPackageId",
                table: "Payments",
                column: "CustomerPackageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_CustomerPackages_CustomerPackageId",
                table: "Payments",
                column: "CustomerPackageId",
                principalTable: "CustomerPackages",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_CustomerPackages_CustomerPackageId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_CustomerPackageId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CustomerPackageId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Discount",
                table: "CustomerPackages");

            migrationBuilder.DropColumn(
                name: "Vat",
                table: "CustomerPackages");
        }
    }
}
