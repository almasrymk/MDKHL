using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SmartParking.Migrations
{
    /// <inheritdoc />
    public partial class addEmaraatable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_Emaraa_EmaraaId",
                table: "Organizations");

            migrationBuilder.DropForeignKey(
                name: "FK_Stats_Emaraa_EmaraaId",
                table: "Stats");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Emaraa",
                table: "Emaraa");

            migrationBuilder.RenameTable(
                name: "Emaraa",
                newName: "Emaraas");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Subscriptions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Stats",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Organizations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "OrganizationPackages",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Customers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EnglishName",
                table: "Emaraas",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ArabicName",
                table: "Emaraas",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Emaraas",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Emaraas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Emaraas",
                table: "Emaraas",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_Emaraas_EmaraaId",
                table: "Organizations",
                column: "EmaraaId",
                principalTable: "Emaraas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Stats_Emaraas_EmaraaId",
                table: "Stats",
                column: "EmaraaId",
                principalTable: "Emaraas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_Emaraas_EmaraaId",
                table: "Organizations");

            migrationBuilder.DropForeignKey(
                name: "FK_Stats_Emaraas_EmaraaId",
                table: "Stats");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Emaraas",
                table: "Emaraas");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Stats");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "OrganizationPackages");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Emaraas");

            migrationBuilder.RenameTable(
                name: "Emaraas",
                newName: "Emaraa");

            migrationBuilder.AlterColumn<string>(
                name: "EnglishName",
                table: "Emaraa",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ArabicName",
                table: "Emaraa",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Emaraa",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Emaraa",
                table: "Emaraa",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_Emaraa_EmaraaId",
                table: "Organizations",
                column: "EmaraaId",
                principalTable: "Emaraa",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Stats_Emaraa_EmaraaId",
                table: "Stats",
                column: "EmaraaId",
                principalTable: "Emaraa",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
