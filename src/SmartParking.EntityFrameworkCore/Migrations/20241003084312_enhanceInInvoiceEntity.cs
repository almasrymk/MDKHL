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
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Packages_PackageId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_PackageId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "PackageId",
                table: "Invoices");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PackageId",
                table: "Invoices",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_PackageId",
                table: "Invoices",
                column: "PackageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Packages_PackageId",
                table: "Invoices",
                column: "PackageId",
                principalTable: "Packages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
