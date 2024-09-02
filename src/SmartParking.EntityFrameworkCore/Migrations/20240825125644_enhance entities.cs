using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SmartParking.Migrations
{
    /// <inheritdoc />
    public partial class enhanceentities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "OrganizationGarages",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "OrganizationGarages",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "OrganizationGarages",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "OrganizationGarages",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "OrganizationGarages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModificationTime",
                table: "OrganizationGarages",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifierId",
                table: "OrganizationGarages",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "OrganizationContactInfos",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "OrganizationContactInfos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "OrganizationContactInfos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "OrganizationContactInfos",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "OrganizationContactInfos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModificationTime",
                table: "OrganizationContactInfos",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifierId",
                table: "OrganizationContactInfos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "CustomerCategories",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "CustomerCategories",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "CustomerCategories",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "CustomerCategories",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "CustomerCategories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModificationTime",
                table: "CustomerCategories",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifierId",
                table: "CustomerCategories",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Emaraa",
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
                    ArabicName = table.Column<string>(type: "text", nullable: false),
                    EnglishName = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Emaraa", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Stats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmaraaId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ArabicName = table.Column<string>(type: "text", nullable: false),
                    EnglishName = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stats_Emaraa_EmaraaId",
                        column: x => x.EmaraaId,
                        principalTable: "Emaraa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_CityId",
                table: "Organizations",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_CountryId",
                table: "Organizations",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_EmaraaId",
                table: "Organizations",
                column: "EmaraaId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_StatsId",
                table: "Organizations",
                column: "StatsId");

            migrationBuilder.CreateIndex(
                name: "IX_Stats_EmaraaId",
                table: "Stats",
                column: "EmaraaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_Cities_CityId",
                table: "Organizations",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_Countries_CountryId",
                table: "Organizations",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_Emaraa_EmaraaId",
                table: "Organizations",
                column: "EmaraaId",
                principalTable: "Emaraa",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_Stats_StatsId",
                table: "Organizations",
                column: "StatsId",
                principalTable: "Stats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_Cities_CityId",
                table: "Organizations");

            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_Countries_CountryId",
                table: "Organizations");

            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_Emaraa_EmaraaId",
                table: "Organizations");

            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_Stats_StatsId",
                table: "Organizations");

            migrationBuilder.DropTable(
                name: "Stats");

            migrationBuilder.DropTable(
                name: "Emaraa");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_CityId",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_CountryId",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_EmaraaId",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_StatsId",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "OrganizationGarages");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "OrganizationGarages");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "OrganizationGarages");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "OrganizationGarages");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "OrganizationGarages");

            migrationBuilder.DropColumn(
                name: "LastModificationTime",
                table: "OrganizationGarages");

            migrationBuilder.DropColumn(
                name: "LastModifierId",
                table: "OrganizationGarages");

            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "OrganizationContactInfos");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "OrganizationContactInfos");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "OrganizationContactInfos");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "OrganizationContactInfos");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "OrganizationContactInfos");

            migrationBuilder.DropColumn(
                name: "LastModificationTime",
                table: "OrganizationContactInfos");

            migrationBuilder.DropColumn(
                name: "LastModifierId",
                table: "OrganizationContactInfos");

            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "CustomerCategories");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "CustomerCategories");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "CustomerCategories");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "CustomerCategories");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "CustomerCategories");

            migrationBuilder.DropColumn(
                name: "LastModificationTime",
                table: "CustomerCategories");

            migrationBuilder.DropColumn(
                name: "LastModifierId",
                table: "CustomerCategories");
        }
    }
}
