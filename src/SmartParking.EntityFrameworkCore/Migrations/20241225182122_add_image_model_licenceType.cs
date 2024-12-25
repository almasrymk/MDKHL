using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParking.Migrations
{
    /// <inheritdoc />
    public partial class add_image_model_licenceType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Image",
                table: "VehicleModels",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Image",
                table: "LicenceTypes",
                type: "text",
                nullable: true);

            migrationBuilder.DropIndex(
             name: "IX_AbpUsers_NormalizedEmail",
             table: "AbpUsers");

            migrationBuilder.AlterColumn<string>(
               name: "NormalizedEmail",
               table: "AbpUsers",
               type: "character varying(256)",
               nullable: true,
               oldClrType: typeof(string),
               oldType: "character varying(256)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Image",
                table: "VehicleModels");

            migrationBuilder.DropColumn(
                name: "Image",
                table: "LicenceTypes");

            migrationBuilder.AlterColumn<string>(
              name: "NormalizedEmail",
              table: "AbpUsers",
              type: "character varying(256)",
              maxLength: 256,
              nullable: false, // Make it required again
              oldClrType: typeof(string),
              oldType: "character varying(256)",
              oldNullable: true);

            // Recreate the unique index on Email column
            migrationBuilder.CreateIndex(
                name: "IX_AbpUsers_NormalizedEmail",
                table: "AbpUsers",
                column: "NormalizedEmail",
                unique: true);
        }
    }
}
