using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParking.Migrations
{
    /// <inheritdoc />
    public partial class addCapitals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ArabicCapital",
                table: "Governorates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnglishCapital",
                table: "Governorates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ArabicCapital",
                table: "Countries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnglishCapital",
                table: "Countries",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArabicCapital",
                table: "Governorates");

            migrationBuilder.DropColumn(
                name: "EnglishCapital",
                table: "Governorates");

            migrationBuilder.DropColumn(
                name: "ArabicCapital",
                table: "Countries");

            migrationBuilder.DropColumn(
                name: "EnglishCapital",
                table: "Countries");
        }
    }
}
