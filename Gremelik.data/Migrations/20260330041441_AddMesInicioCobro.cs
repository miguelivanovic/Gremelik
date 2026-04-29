using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gremelik.data.Migrations
{
    /// <inheritdoc />
    public partial class AddMesInicioCobro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MesInicioCobro",
                table: "PlanesPago",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MesInicioCobro",
                table: "PlanesPago");
        }
    }
}
