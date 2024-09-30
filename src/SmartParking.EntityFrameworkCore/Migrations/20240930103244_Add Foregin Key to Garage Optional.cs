using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParking.Migrations
{
    /// <inheritdoc />
    public partial class AddForeginKeytoGarageOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cameras_Garages_GarageId",
                table: "Cameras");

            migrationBuilder.AlterColumn<int>(
                name: "GarageId",
                table: "Cameras",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Cameras_Garages_GarageId",
                table: "Cameras",
                column: "GarageId",
                principalTable: "Garages",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cameras_Garages_GarageId",
                table: "Cameras");

            migrationBuilder.AlterColumn<int>(
                name: "GarageId",
                table: "Cameras",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Cameras_Garages_GarageId",
                table: "Cameras",
                column: "GarageId",
                principalTable: "Garages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
