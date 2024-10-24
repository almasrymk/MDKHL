using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SmartParking.Migrations
{
    /// <inheritdoc />
    public partial class addrelationcustomerOrganizations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_AbpUsers_IdentityUserId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Vehicles_VehicleId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_Customers_CustomerId",
                table: "Organizations");

            migrationBuilder.DropTable(
                name: "OrganizationCustomers");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_CustomerId",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_VehicleId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "VehicleId",
                table: "Invoices");

            migrationBuilder.RenameColumn(
                name: "PlateNnumber",
                table: "GarageTransactions",
                newName: "PlateNumber");

            migrationBuilder.AddColumn<string>(
                name: "PlateNumber",
                table: "Invoices",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "IdentityUserId",
                table: "Customers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "CustomerOrganizations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
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
                name: "FK_Customers_AbpUsers_IdentityUserId",
                table: "Customers",
                column: "IdentityUserId",
                principalTable: "AbpUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_AbpUsers_IdentityUserId",
                table: "Customers");

            migrationBuilder.DropTable(
                name: "CustomerOrganizations");

            migrationBuilder.DropColumn(
                name: "PlateNumber",
                table: "Invoices");

            migrationBuilder.RenameColumn(
                name: "PlateNumber",
                table: "GarageTransactions",
                newName: "PlateNnumber");

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "Organizations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VehicleId",
                table: "Invoices",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<Guid>(
                name: "IdentityUserId",
                table: "Customers",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateTable(
                name: "OrganizationCustomers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationCustomers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_CustomerId",
                table: "Organizations",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_VehicleId",
                table: "Invoices",
                column: "VehicleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_AbpUsers_IdentityUserId",
                table: "Customers",
                column: "IdentityUserId",
                principalTable: "AbpUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Vehicles_VehicleId",
                table: "Invoices",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_Customers_CustomerId",
                table: "Organizations",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id");
        }
    }
}
