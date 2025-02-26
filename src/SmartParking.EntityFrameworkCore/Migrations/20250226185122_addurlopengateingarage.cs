using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParking.Migrations
{
    /// <inheritdoc />
    public partial class addurlopengateingarage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OpenGateURL",
                table: "Garages",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OpenGateURL",
                table: "Garages");
        }
    }
}
