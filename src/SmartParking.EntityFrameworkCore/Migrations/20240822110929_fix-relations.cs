using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParking.Migrations
{
    /// <inheritdoc />
    public partial class fixrelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cities_Governorates_GovernorateId1",
                table: "Cities");

            migrationBuilder.DropForeignKey(
                name: "FK_Governorates_Countries_CountryId1",
                table: "Governorates");

            migrationBuilder.DropIndex(
                name: "IX_Governorates_CountryId1",
                table: "Governorates");

            migrationBuilder.DropIndex(
                name: "IX_Cities_GovernorateId1",
                table: "Cities");

            migrationBuilder.DropColumn(
                name: "CountryId1",
                table: "Governorates");

            migrationBuilder.DropColumn(
                name: "GovernorateId1",
                table: "Cities");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CountryId1",
                table: "Governorates",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GovernorateId1",
                table: "Cities",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Governorates_CountryId1",
                table: "Governorates",
                column: "CountryId1");

            migrationBuilder.CreateIndex(
                name: "IX_Cities_GovernorateId1",
                table: "Cities",
                column: "GovernorateId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Cities_Governorates_GovernorateId1",
                table: "Cities",
                column: "GovernorateId1",
                principalTable: "Governorates",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Governorates_Countries_CountryId1",
                table: "Governorates",
                column: "CountryId1",
                principalTable: "Countries",
                principalColumn: "Id");
        }
    }
}
