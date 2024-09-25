using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SmartParking.Migrations
{
    /// <inheritdoc />
    public partial class addRegionEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_Emaraas_EmaraaId",
                table: "Organizations");

            migrationBuilder.DropForeignKey(
                name: "FK_State_Emaraas_EmaraaId",
                table: "State");

            migrationBuilder.DropTable(
                name: "Emaraas");

            migrationBuilder.RenameColumn(
                name: "EmaraaId",
                table: "State",
                newName: "RegionId");

            migrationBuilder.RenameIndex(
                name: "IX_State_EmaraaId",
                table: "State",
                newName: "IX_State_RegionId");

            migrationBuilder.RenameColumn(
                name: "EmaraaId",
                table: "Organizations",
                newName: "RegionId");

            migrationBuilder.RenameIndex(
                name: "IX_Organizations_EmaraaId",
                table: "Organizations",
                newName: "IX_Organizations_RegionId");

            migrationBuilder.RenameColumn(
                name: "EmaraaId",
                table: "Cities",
                newName: "RegionId");

            migrationBuilder.CreateTable(
                name: "Regions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CountryId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ArabicName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EnglishName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regions", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_Regions_RegionId",
                table: "Organizations",
                column: "RegionId",
                principalTable: "Regions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_State_Regions_RegionId",
                table: "State",
                column: "RegionId",
                principalTable: "Regions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_Regions_RegionId",
                table: "Organizations");

            migrationBuilder.DropForeignKey(
                name: "FK_State_Regions_RegionId",
                table: "State");

            migrationBuilder.DropTable(
                name: "Regions");

            migrationBuilder.RenameColumn(
                name: "RegionId",
                table: "State",
                newName: "EmaraaId");

            migrationBuilder.RenameIndex(
                name: "IX_State_RegionId",
                table: "State",
                newName: "IX_State_EmaraaId");

            migrationBuilder.RenameColumn(
                name: "RegionId",
                table: "Organizations",
                newName: "EmaraaId");

            migrationBuilder.RenameIndex(
                name: "IX_Organizations_RegionId",
                table: "Organizations",
                newName: "IX_Organizations_EmaraaId");

            migrationBuilder.RenameColumn(
                name: "RegionId",
                table: "Cities",
                newName: "EmaraaId");

            migrationBuilder.CreateTable(
                name: "Emaraas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ArabicName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CountryId = table.Column<int>(type: "integer", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    EnglishName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Emaraas", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_Emaraas_EmaraaId",
                table: "Organizations",
                column: "EmaraaId",
                principalTable: "Emaraas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_State_Emaraas_EmaraaId",
                table: "State",
                column: "EmaraaId",
                principalTable: "Emaraas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
