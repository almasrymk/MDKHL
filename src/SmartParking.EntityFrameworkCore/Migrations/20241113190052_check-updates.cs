using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParking.Migrations
{
    /// <inheritdoc />
    public partial class checkupdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "SubscriptionTypes",
                newName: "TypeEn");

            migrationBuilder.AddColumn<int>(
                name: "SubscriptionStatus",
                table: "SubscriptionTypes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TypeAr",
                table: "SubscriptionTypes",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubscriptionStatus",
                table: "SubscriptionTypes");

            migrationBuilder.DropColumn(
                name: "TypeAr",
                table: "SubscriptionTypes");

            migrationBuilder.RenameColumn(
                name: "TypeEn",
                table: "SubscriptionTypes",
                newName: "Type");
        }
    }
}
