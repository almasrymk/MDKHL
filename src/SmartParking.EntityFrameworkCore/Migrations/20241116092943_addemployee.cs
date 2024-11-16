using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SmartParking.Migrations
{
    /// <inheritdoc />
    public partial class addemployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerPackages_Packages_PackageId",
                table: "CustomerPackages");

            migrationBuilder.DropTable(
                name: "CustomerOrganizations");

            migrationBuilder.RenameColumn(
                name: "NumberOfOrganizations",
                table: "Subscriptions",
                newName: "ParkingSlot");

            migrationBuilder.RenameColumn(
                name: "NumberOfMobileCustomers",
                table: "Subscriptions",
                newName: "NumberOfCustomers");

            migrationBuilder.AddColumn<int>(
                name: "UsedSlots",
                table: "Organizations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrnetCapacityEmployees",
                table: "Garages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrnetCapacityForCustomers",
                table: "Garages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrnetCapacityForFree",
                table: "Garages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxSlotsForCustomers",
                table: "Garages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxSlotsForEmployess",
                table: "Garages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxSlotsForFree",
                table: "Garages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmployee",
                table: "Customers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "PackageId",
                table: "CustomerPackages",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "CustomerPackages",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerPackages_OrganizationId",
                table: "CustomerPackages",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerPackages_Organizations_OrganizationId",
                table: "CustomerPackages",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerPackages_Packages_PackageId",
                table: "CustomerPackages",
                column: "PackageId",
                principalTable: "Packages",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerPackages_Organizations_OrganizationId",
                table: "CustomerPackages");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerPackages_Packages_PackageId",
                table: "CustomerPackages");

            migrationBuilder.DropIndex(
                name: "IX_CustomerPackages_OrganizationId",
                table: "CustomerPackages");

            migrationBuilder.DropColumn(
                name: "UsedSlots",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "CurrnetCapacityEmployees",
                table: "Garages");

            migrationBuilder.DropColumn(
                name: "CurrnetCapacityForCustomers",
                table: "Garages");

            migrationBuilder.DropColumn(
                name: "CurrnetCapacityForFree",
                table: "Garages");

            migrationBuilder.DropColumn(
                name: "MaxSlotsForCustomers",
                table: "Garages");

            migrationBuilder.DropColumn(
                name: "MaxSlotsForEmployess",
                table: "Garages");

            migrationBuilder.DropColumn(
                name: "MaxSlotsForFree",
                table: "Garages");

            migrationBuilder.DropColumn(
                name: "IsEmployee",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "CustomerPackages");

            migrationBuilder.RenameColumn(
                name: "ParkingSlot",
                table: "Subscriptions",
                newName: "NumberOfOrganizations");

            migrationBuilder.RenameColumn(
                name: "NumberOfCustomers",
                table: "Subscriptions",
                newName: "NumberOfMobileCustomers");

            migrationBuilder.AlterColumn<int>(
                name: "PackageId",
                table: "CustomerPackages",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "CustomerOrganizations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerOrganizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerOrganizations_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerOrganizations_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOrganizations_CustomerId",
                table: "CustomerOrganizations",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOrganizations_OrganizationId",
                table: "CustomerOrganizations",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerPackages_Packages_PackageId",
                table: "CustomerPackages",
                column: "PackageId",
                principalTable: "Packages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
