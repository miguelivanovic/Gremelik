using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gremelik.data.Migrations
{
    /// <inheritdoc />
    public partial class EscalasCalificacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CalificacionMinima",
                table: "ConfiguracionesAcademicas",
                newName: "EscalaMinima");

            migrationBuilder.AddColumn<decimal>(
                name: "CalificacionAprobatoria",
                table: "ConfiguracionesAcademicas",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EscalaMaxima",
                table: "ConfiguracionesAcademicas",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CalificacionAprobatoria",
                table: "ConfiguracionesAcademicas");

            migrationBuilder.DropColumn(
                name: "EscalaMaxima",
                table: "ConfiguracionesAcademicas");

            migrationBuilder.RenameColumn(
                name: "EscalaMinima",
                table: "ConfiguracionesAcademicas",
                newName: "CalificacionMinima");
        }
    }
}
