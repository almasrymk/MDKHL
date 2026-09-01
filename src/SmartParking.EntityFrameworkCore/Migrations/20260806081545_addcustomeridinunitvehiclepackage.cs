using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParking.Migrations
{
    /// <inheritdoc />
    public partial class addcustomeridinunitvehiclepackage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UnitVehiclePackges_CustomerId",
                table: "UnitVehiclePackges",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitVehiclePackges_CustomerUnitPackageId",
                table: "UnitVehiclePackges",
                column: "CustomerUnitPackageId");

            migrationBuilder.AddForeignKey(
                name: "FK_UnitVehiclePackges_CustomerUnitPackages_CustomerUnitPackage~",
                table: "UnitVehiclePackges",
                column: "CustomerUnitPackageId",
                principalTable: "CustomerUnitPackages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UnitVehiclePackges_Customers_CustomerId",
                table: "UnitVehiclePackges",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UnitVehiclePackges_CustomerUnitPackages_CustomerUnitPackage~",
                table: "UnitVehiclePackges");

            migrationBuilder.DropForeignKey(
                name: "FK_UnitVehiclePackges_Customers_CustomerId",
                table: "UnitVehiclePackges");

            migrationBuilder.DropIndex(
                name: "IX_UnitVehiclePackges_CustomerId",
                table: "UnitVehiclePackges");

            migrationBuilder.DropIndex(
                name: "IX_UnitVehiclePackges_CustomerUnitPackageId",
                table: "UnitVehiclePackges");
        }
    }
}
