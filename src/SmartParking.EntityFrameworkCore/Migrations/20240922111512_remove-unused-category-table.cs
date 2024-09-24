using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SmartParking.Migrations
{
    /// <inheritdoc />
    public partial class removeunusedcategorytable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerCategory_Organizations_OrganizationId",
                table: "CustomerCategory");

            migrationBuilder.DropForeignKey(
                name: "FK_PackageCategories_Categories_CategoryId",
                table: "PackageCategories");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CustomerCategory",
                table: "CustomerCategory");

            migrationBuilder.RenameTable(
                name: "CustomerCategory",
                newName: "CustomerCategories");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "PackageCategories",
                newName: "CustomerCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_PackageCategories_CategoryId",
                table: "PackageCategories",
                newName: "IX_PackageCategories_CustomerCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_CustomerCategory_OrganizationId",
                table: "CustomerCategories",
                newName: "IX_CustomerCategories_OrganizationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomerCategories",
                table: "CustomerCategories",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerCategories_Organizations_OrganizationId",
                table: "CustomerCategories",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PackageCategories_CustomerCategories_CustomerCategoryId",
                table: "PackageCategories",
                column: "CustomerCategoryId",
                principalTable: "CustomerCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerCategories_Organizations_OrganizationId",
                table: "CustomerCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_PackageCategories_CustomerCategories_CustomerCategoryId",
                table: "PackageCategories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CustomerCategories",
                table: "CustomerCategories");

            migrationBuilder.RenameTable(
                name: "CustomerCategories",
                newName: "CustomerCategory");

            migrationBuilder.RenameColumn(
                name: "CustomerCategoryId",
                table: "PackageCategories",
                newName: "CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_PackageCategories_CustomerCategoryId",
                table: "PackageCategories",
                newName: "IX_PackageCategories_CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_CustomerCategories_OrganizationId",
                table: "CustomerCategory",
                newName: "IX_CustomerCategory_OrganizationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomerCategory",
                table: "CustomerCategory",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    Default = table.Column<bool>(type: "boolean", nullable: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerCategory_Organizations_OrganizationId",
                table: "CustomerCategory",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PackageCategories_Categories_CategoryId",
                table: "PackageCategories",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
