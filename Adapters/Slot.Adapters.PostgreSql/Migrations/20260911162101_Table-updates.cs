using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Slot.Adapters.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class Tableupdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "Grounds",
                type: "numeric(12,9)",
                precision: 12,
                scale: 9,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,7)",
                oldPrecision: 10,
                oldScale: 7);

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "Grounds",
                type: "numeric(12,9)",
                precision: 12,
                scale: 9,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,7)",
                oldPrecision: 10,
                oldScale: 7);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "Grounds",
                type: "numeric(10,7)",
                precision: 10,
                scale: 7,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(12,9)",
                oldPrecision: 12,
                oldScale: 9);

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "Grounds",
                type: "numeric(10,7)",
                precision: 10,
                scale: 7,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(12,9)",
                oldPrecision: 12,
                oldScale: 9);
        }
    }
}
