using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParking.Migrations
{
    /// <inheritdoc />
    public partial class addCameraInOrganization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Cameras_OrganizationId",
                table: "Cameras",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cameras_Organizations_OrganizationId",
                table: "Cameras",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cameras_Organizations_OrganizationId",
                table: "Cameras");

            migrationBuilder.DropIndex(
                name: "IX_Cameras_OrganizationId",
                table: "Cameras");
        }
    }
}
