using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParking.Migrations
{
    /// <inheritdoc />
    public partial class AddCameraEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cameras_Organizations_OrganizationId",
                table: "Cameras");

            migrationBuilder.RenameColumn(
                name: "OrganizationGarageId",
                table: "Cameras",
                newName: "GarageId");

            migrationBuilder.AlterColumn<int>(
                name: "OrganizationId",
                table: "Cameras",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "BrandName",
                table: "Cameras",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "Cameras",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "Cameras",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "Cameras",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "Cameras",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Cameras",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModificationTime",
                table: "Cameras",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifierId",
                table: "Cameras",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MacAddress",
                table: "Cameras",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MainType",
                table: "Cameras",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "Cameras",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Cameras",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Cameras",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Cameras_Organizations_OrganizationId",
                table: "Cameras",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cameras_Organizations_OrganizationId",
                table: "Cameras");

            migrationBuilder.DropColumn(
                name: "BrandName",
                table: "Cameras");

            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "Cameras");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Cameras");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "Cameras");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "Cameras");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Cameras");

            migrationBuilder.DropColumn(
                name: "LastModificationTime",
                table: "Cameras");

            migrationBuilder.DropColumn(
                name: "LastModifierId",
                table: "Cameras");

            migrationBuilder.DropColumn(
                name: "MacAddress",
                table: "Cameras");

            migrationBuilder.DropColumn(
                name: "MainType",
                table: "Cameras");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "Cameras");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Cameras");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Cameras");

            migrationBuilder.RenameColumn(
                name: "GarageId",
                table: "Cameras",
                newName: "OrganizationGarageId");

            migrationBuilder.AlterColumn<int>(
                name: "OrganizationId",
                table: "Cameras",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Cameras_Organizations_OrganizationId",
                table: "Cameras",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
