using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParking.Migrations
{
    /// <inheritdoc />
    public partial class enhnaceInsubscriptionEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SubscriptionName",
                table: "Subscriptions",
                newName: "Notes");

            migrationBuilder.AddColumn<string>(
                name: "ArabicName",
                table: "Subscriptions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnglishName",
                table: "Subscriptions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArabicName",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "EnglishName",
                table: "Subscriptions");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "Subscriptions",
                newName: "SubscriptionName");
        }
    }
}
