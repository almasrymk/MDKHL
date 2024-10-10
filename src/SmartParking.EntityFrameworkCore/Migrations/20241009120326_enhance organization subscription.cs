using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParking.Migrations
{
    /// <inheritdoc />
    public partial class enhanceorganizationsubscription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_Organizations_Subscriptions_SubscriptionId",
            //    table: "Organizations");

            //migrationBuilder.AlterColumn<int>(
            //    name: "SubscriptionId",
            //    table: "Organizations",
            //    type: "integer",
            //    nullable: true,
            //    oldClrType: typeof(int),
            //    oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_Subscriptions_SubscriptionId",
                table: "Organizations",
                column: "SubscriptionId",
                principalTable: "Subscriptions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_Subscriptions_SubscriptionId",
                table: "Organizations");

            migrationBuilder.AlterColumn<int>(
                name: "SubscriptionId",
                table: "Organizations",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_Subscriptions_SubscriptionId",
                table: "Organizations",
                column: "SubscriptionId",
                principalTable: "Subscriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
