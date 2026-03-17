using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParking.Migrations
{
    /// <inheritdoc />
    public partial class renterandownercolumnsunit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Capacity",
                table: "Units",
                newName: "RenterSlots");

            migrationBuilder.AddColumn<int>(
                name: "OwnerSlots",
                table: "Units",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerSlots",
                table: "Units");

            migrationBuilder.RenameColumn(
                name: "RenterSlots",
                table: "Units",
                newName: "Capacity");
        }
    }
}
