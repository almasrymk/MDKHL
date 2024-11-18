using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParking.Migrations
{
    /// <inheritdoc />
    public partial class cameraedits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Model",
                table: "Cameras");
            migrationBuilder.AddColumn<int>(
                name: "Model",
                table: "Cameras",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.DropColumn(
                name: "MainType",
                table: "Cameras");
            migrationBuilder.AddColumn<int>(
                name: "MainType",
                table: "Cameras",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.DropColumn(
                name: "BrandName",
                table: "Cameras");
            migrationBuilder.AddColumn<int>(
                name: "BrandName",
                table: "Cameras",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Model",
                table: "Cameras",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "MainType",
                table: "Cameras",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "BrandName",
                table: "Cameras",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
