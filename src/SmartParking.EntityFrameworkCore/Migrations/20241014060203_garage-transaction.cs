using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParking.Migrations
{
    /// <inheritdoc />
    public partial class garagetransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "GarageTransactions");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Cameras");

            migrationBuilder.AddColumn<int>(
                name: "CameraType",
                table: "GarageTransactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrnetCapicity",
                table: "Garages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CameraType",
                table: "Cameras",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CameraType",
                table: "GarageTransactions");

            migrationBuilder.DropColumn(
                name: "CurrnetCapicity",
                table: "Garages");

            migrationBuilder.DropColumn(
                name: "CameraType",
                table: "Cameras");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "GarageTransactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Cameras",
                type: "text",
                nullable: true);
        }
    }
}
