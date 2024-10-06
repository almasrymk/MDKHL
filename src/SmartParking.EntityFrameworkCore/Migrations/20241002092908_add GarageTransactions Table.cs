using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SmartParking.Migrations
{
    /// <inheritdoc />
    public partial class addGarageTransactionsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GarageTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CameraId = table.Column<int>(type: "integer", nullable: false),
                    PlateNnumber = table.Column<string>(type: "text", nullable: true),
                    SnapTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    AccurateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    OrganizationID = table.Column<int>(type: "integer", nullable: false),
                    GarageId = table.Column<int>(type: "integer", nullable: false),
                    VehiclePicture = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_GarageTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GarageTransactions_Cameras_CameraId",
                        column: x => x.CameraId,
                        principalTable: "Cameras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GarageTransactions_Garages_GarageId",
                        column: x => x.GarageId,
                        principalTable: "Garages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GarageTransactions_Organizations_OrganizationID",
                        column: x => x.OrganizationID,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cameras_GarageId",
                table: "Cameras",
                column: "GarageId");

            migrationBuilder.CreateIndex(
                name: "IX_GarageTransactions_CameraId",
                table: "GarageTransactions",
                column: "CameraId");

            migrationBuilder.CreateIndex(
                name: "IX_GarageTransactions_GarageId",
                table: "GarageTransactions",
                column: "GarageId");

            migrationBuilder.CreateIndex(
                name: "IX_GarageTransactions_OrganizationID",
                table: "GarageTransactions",
                column: "OrganizationID");

            migrationBuilder.AddForeignKey(
                name: "FK_Cameras_Garages_GarageId",
                table: "Cameras",
                column: "GarageId",
                principalTable: "Garages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cameras_Garages_GarageId",
                table: "Cameras");

            migrationBuilder.DropTable(
                name: "GarageTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Cameras_GarageId",
                table: "Cameras");
        }
    }
}
