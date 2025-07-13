using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParking.Migrations
{
    /// <inheritdoc />
    public partial class addingdispenserchangesinorganization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GarageTransactions_Cameras_CameraId",
                table: "GarageTransactions");

            migrationBuilder.AddColumn<bool>(
                name: "CameraTransaction",
                table: "Organizations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DispenserTransaction",
                table: "Organizations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "CameraId",
                table: "GarageTransactions",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_GarageTransactions_Cameras_CameraId",
                table: "GarageTransactions",
                column: "CameraId",
                principalTable: "Cameras",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GarageTransactions_Cameras_CameraId",
                table: "GarageTransactions");

            migrationBuilder.DropColumn(
                name: "CameraTransaction",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "DispenserTransaction",
                table: "Organizations");

            migrationBuilder.AlterColumn<int>(
                name: "CameraId",
                table: "GarageTransactions",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GarageTransactions_Cameras_CameraId",
                table: "GarageTransactions",
                column: "CameraId",
                principalTable: "Cameras",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
