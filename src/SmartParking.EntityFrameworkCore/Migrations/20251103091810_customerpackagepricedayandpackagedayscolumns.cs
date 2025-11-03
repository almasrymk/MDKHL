using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParking.Migrations
{
    /// <inheritdoc />
    public partial class customerpackagepricedayandpackagedayscolumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CountOfDays",
                table: "CustomerPackages",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PriceOfPackagePerDay",
                table: "CustomerPackages",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CountOfDays",
                table: "CustomerPackages");

            migrationBuilder.DropColumn(
                name: "PriceOfPackagePerDay",
                table: "CustomerPackages");
        }
    }
}
