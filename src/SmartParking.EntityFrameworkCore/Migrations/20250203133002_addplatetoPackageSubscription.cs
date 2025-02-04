using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParking.Migrations
{
    /// <inheritdoc />
    public partial class addplatetoPackageSubscription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ArPlateNumber",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnPlateNumber",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ArPlateNumber",
                table: "CustomerPackages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnPlateNumber",
                table: "CustomerPackages",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArPlateNumber",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "EnPlateNumber",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ArPlateNumber",
                table: "CustomerPackages");

            migrationBuilder.DropColumn(
                name: "EnPlateNumber",
                table: "CustomerPackages");
        }
    }
}
