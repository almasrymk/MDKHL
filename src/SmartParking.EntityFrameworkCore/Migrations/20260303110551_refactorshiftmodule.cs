using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParking.Migrations
{
    /// <inheritdoc />
    public partial class refactorshiftmodule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "GarageId",
                table: "Shifts",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateTable(
                name: "ShiftZones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShiftId = table.Column<int>(type: "integer", nullable: false),
                    ShiftArabicName = table.Column<string>(type: "text", nullable: true),
                    ShiftEnglishName = table.Column<string>(type: "text", nullable: true),
                    ZoneId = table.Column<int>(type: "integer", nullable: false),
                    ZoneArName = table.Column<string>(type: "text", nullable: true),
                    ZoneEnName = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    OrganizationArName = table.Column<string>(type: "text", nullable: true),
                    OrganizationEnName = table.Column<string>(type: "text", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    IsFullDay = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ShiftManager_ManagerEnName = table.Column<string>(type: "text", nullable: true),
                    ShiftManager_ManagerPhoneNumber = table.Column<string>(type: "text", nullable: true),
                    ShiftManager_ManagerUserName = table.Column<string>(type: "text", nullable: true),
                    ShiftManager_ShiftManagerId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_ShiftZones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShiftDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShiftZoneId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_ShiftDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShiftDetails_ShiftZones_ShiftZoneId",
                        column: x => x.ShiftZoneId,
                        principalTable: "ShiftZones",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ShiftDevices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<int>(type: "integer", nullable: false),
                    DeviceCode = table.Column<string>(type: "text", nullable: true),
                    DeviceUserName = table.Column<string>(type: "text", nullable: true),
                    DevicePassword = table.Column<string>(type: "text", nullable: true),
                    ShiftDetailsId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_ShiftDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShiftDevices_ShiftDetails_ShiftDetailsId",
                        column: x => x.ShiftDetailsId,
                        principalTable: "ShiftDetails",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ShiftStaffDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShiftDetailsId = table.Column<Guid>(type: "uuid", nullable: true),
                    StaffEnName = table.Column<string>(type: "text", nullable: true),
                    StaffArName = table.Column<string>(type: "text", nullable: true),
                    StaffPhoneNumber = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_ShiftStaffDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShiftStaffDetails_ShiftDetails_ShiftDetailsId",
                        column: x => x.ShiftDetailsId,
                        principalTable: "ShiftDetails",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShiftDetails_ShiftZoneId",
                table: "ShiftDetails",
                column: "ShiftZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftDevices_ShiftDetailsId",
                table: "ShiftDevices",
                column: "ShiftDetailsId",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftStaffDetails_ShiftDetailsId",
                table: "ShiftStaffDetails",
                column: "ShiftDetailsId",
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShiftDevices");

            migrationBuilder.DropTable(
                name: "ShiftStaffDetails");

            migrationBuilder.DropTable(
                name: "ShiftDetails");

            migrationBuilder.DropTable(
                name: "ShiftZones");

            migrationBuilder.AlterColumn<int>(
                name: "GarageId",
                table: "Shifts",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
