using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SmartParking.Migrations
{
    /// <inheritdoc />
    public partial class enhanceInOrganizationEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DefaultPackages");

            migrationBuilder.AddColumn<decimal>(
                name: "AdditionalHourPrice",
                table: "Organizations",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "ExceededTime",
                table: "Organizations",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<decimal>(
                name: "FirstHourPrice",
                table: "Organizations",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Fullday",
                table: "Organizations",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsFullDayExceeded",
                table: "Organizations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsNumberOfHoursExceeded",
                table: "Organizations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfHours",
                table: "Organizations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "SecoundHourPrice",
                table: "Organizations",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ThirdHourPrice",
                table: "Organizations",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalHourPrice",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "ExceededTime",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "FirstHourPrice",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "Fullday",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "IsFullDayExceeded",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "IsNumberOfHoursExceeded",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "NumberOfHours",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "SecoundHourPrice",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "ThirdHourPrice",
                table: "Organizations");

            migrationBuilder.CreateTable(
                name: "DefaultPackages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganizationId = table.Column<int>(type: "integer", nullable: true),
                    AdditionalHourPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    ArabicName = table.Column<string>(type: "text", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    EnglishName = table.Column<string>(type: "text", nullable: true),
                    ExceededTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    FirstHourPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    Fullday = table.Column<decimal>(type: "numeric", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsFullDayExceeded = table.Column<bool>(type: "boolean", nullable: false),
                    IsNumberOfHoursExceeded = table.Column<bool>(type: "boolean", nullable: false),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    NumberOfHours = table.Column<int>(type: "integer", nullable: false),
                    OrgnizationId = table.Column<int>(type: "integer", nullable: false),
                    SecoundHourPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ThirdHourPrice = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefaultPackages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DefaultPackages_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DefaultPackages_OrganizationId",
                table: "DefaultPackages",
                column: "OrganizationId");
        }
    }
}
