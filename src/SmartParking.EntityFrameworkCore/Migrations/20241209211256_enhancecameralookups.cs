using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParking.Migrations
{
    /// <inheritdoc />
    public partial class enhancecameralookups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cameras_CameraBrands_BrandNameId",
                table: "Cameras");

            migrationBuilder.DropForeignKey(
                name: "FK_Cameras_CameraMainTypes_MainTypeId",
                table: "Cameras");

            migrationBuilder.DropForeignKey(
                name: "FK_Cameras_CameraModels_ModelId",
                table: "Cameras");

            migrationBuilder.AlterColumn<int>(
                name: "ModelId",
                table: "Cameras",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MainTypeId",
                table: "Cameras",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "BrandNameId",
                table: "Cameras",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Cameras_CameraBrands_BrandNameId",
                table: "Cameras",
                column: "BrandNameId",
                principalTable: "CameraBrands",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetDefault);

            migrationBuilder.AddForeignKey(
                name: "FK_Cameras_CameraMainTypes_MainTypeId",
                table: "Cameras",
                column: "MainTypeId",
                principalTable: "CameraMainTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetDefault);

            migrationBuilder.AddForeignKey(
                name: "FK_Cameras_CameraModels_ModelId",
                table: "Cameras",
                column: "ModelId",
                principalTable: "CameraModels",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetDefault);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cameras_CameraBrands_BrandNameId",
                table: "Cameras");

            migrationBuilder.DropForeignKey(
                name: "FK_Cameras_CameraMainTypes_MainTypeId",
                table: "Cameras");

            migrationBuilder.DropForeignKey(
                name: "FK_Cameras_CameraModels_ModelId",
                table: "Cameras");

            migrationBuilder.AlterColumn<int>(
                name: "ModelId",
                table: "Cameras",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "MainTypeId",
                table: "Cameras",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "BrandNameId",
                table: "Cameras",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Cameras_CameraBrands_BrandNameId",
                table: "Cameras",
                column: "BrandNameId",
                principalTable: "CameraBrands",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Cameras_CameraMainTypes_MainTypeId",
                table: "Cameras",
                column: "MainTypeId",
                principalTable: "CameraMainTypes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Cameras_CameraModels_ModelId",
                table: "Cameras",
                column: "ModelId",
                principalTable: "CameraModels",
                principalColumn: "Id");
        }
    }
}
