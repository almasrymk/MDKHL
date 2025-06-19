using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartParking.Migrations
{
    /// <inheritdoc />
    public partial class addsequenceindispensertable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateSequence<int>(
                name: "dispenser_seq",
                startValue: 1000L);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Dispensers",
                type: "integer",
                nullable: false,
                defaultValueSql: "nextval('dispenser_seq')",
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValueSql: "nextval('\"WatcherSequence\"')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropSequence(
                name: "dispenser_seq");


            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Dispensers",
                type: "integer",
                nullable: false,
                defaultValueSql: "nextval('\"WatcherSequence\"')",
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValueSql: "nextval('dispenser_seq')");

        }
    }
}
