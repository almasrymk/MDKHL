using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParking.Migrations
{
    /// <inheritdoc />
    public partial class dropIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropIndex(
             name: "IX_AbpUsers_Email",
             table: "AbpUsers");

            migrationBuilder.AlterColumn<string>(
               name: "Email",
               table: "AbpUsers",
               type: "character varying(256)",
               nullable: true,
               oldClrType: typeof(string),
               oldType: "character varying(256)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "AbpUsers",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false, // Make it required again
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldNullable: true);

            // Recreate the unique index on Email column
            migrationBuilder.CreateIndex(
                name: "IX_AbpUsers_Email",
                table: "AbpUsers",
                column: "Email",
                unique: true);
        }

    }
}
