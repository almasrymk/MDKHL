using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SmartParking.Migrations
{
    /// <inheritdoc />
    public partial class cameralookups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BrandName",
                table: "Cameras");

            migrationBuilder.DropColumn(
                name: "MainType",
                table: "Cameras");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "Cameras");

            migrationBuilder.AddColumn<int>(
                name: "BrandNameId",
                table: "Cameras",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MainTypeId",
                table: "Cameras",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ModelId",
                table: "Cameras",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CameraBrands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ArabicName = table.Column<string>(type: "text", nullable: true),
                    EnglishName = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CameraBrands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CameraMainTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CameraBrandId = table.Column<int>(type: "integer", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ArabicName = table.Column<string>(type: "text", nullable: true),
                    EnglishName = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CameraMainTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CameraMainTypes_CameraBrands_CameraBrandId",
                        column: x => x.CameraBrandId,
                        principalTable: "CameraBrands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CameraModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CameraBrandId = table.Column<int>(type: "integer", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ArabicName = table.Column<string>(type: "text", nullable: true),
                    EnglishName = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CameraModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CameraModels_CameraBrands_CameraBrandId",
                        column: x => x.CameraBrandId,
                        principalTable: "CameraBrands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cameras_BrandNameId",
                table: "Cameras",
                column: "BrandNameId");

            migrationBuilder.CreateIndex(
                name: "IX_Cameras_MainTypeId",
                table: "Cameras",
                column: "MainTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Cameras_ModelId",
                table: "Cameras",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_CameraMainTypes_CameraBrandId",
                table: "CameraMainTypes",
                column: "CameraBrandId");

            migrationBuilder.CreateIndex(
                name: "IX_CameraModels_CameraBrandId",
                table: "CameraModels",
                column: "CameraBrandId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cameras_CameraBrands_BrandNameId",
                table: "Cameras",
                column: "BrandNameId",
                principalTable: "CameraBrands",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Cameras_CameraMainTypes_MainTypeId",
                table: "Cameras",
                column: "MainTypeId",
                principalTable: "CameraMainTypes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Cameras_CameraModels_ModelId",
                table: "Cameras",
                column: "ModelId",
                principalTable: "CameraModels",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cameras_CameraBrands_BrandNameId",
                table: "Cameras");

            migrationBuilder.DropForeignKey(
                name: "FK_Cameras_CameraMainTypes_MainTypeId",
                table: "Cameras");

            migrationBuilder.DropForeignKey(
                name: "FK_Cameras_CameraModels_ModelId",
                table: "Cameras");

            migrationBuilder.DropTable(
                name: "CameraMainTypes");

            migrationBuilder.DropTable(
                name: "CameraModels");

            migrationBuilder.DropTable(
                name: "CameraBrands");

            migrationBuilder.DropIndex(
                name: "IX_Cameras_BrandNameId",
                table: "Cameras");

            migrationBuilder.DropIndex(
                name: "IX_Cameras_MainTypeId",
                table: "Cameras");

            migrationBuilder.DropIndex(
                name: "IX_Cameras_ModelId",
                table: "Cameras");

            migrationBuilder.DropColumn(
                name: "BrandNameId",
                table: "Cameras");

            migrationBuilder.DropColumn(
                name: "MainTypeId",
                table: "Cameras");

            migrationBuilder.DropColumn(
                name: "ModelId",
                table: "Cameras");

            migrationBuilder.AddColumn<int>(
                name: "BrandName",
                table: "Cameras",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MainType",
                table: "Cameras",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Model",
                table: "Cameras",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
