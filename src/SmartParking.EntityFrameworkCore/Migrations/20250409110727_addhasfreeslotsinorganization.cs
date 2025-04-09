using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParking.Migrations
{
    /// <inheritdoc />
    public partial class addhasfreeslotsinorganization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasFreeSlots",
                table: "Organizations",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasFreeSlots",
                table: "Organizations");
        }
    }
}
