using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gremelik.data.Migrations
{
    /// <inheritdoc />
    public partial class ConceptosPorFrecuencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EsColegiatura",
                table: "ConceptosPago");

            migrationBuilder.DropColumn(
                name: "EsInscripcion",
                table: "ConceptosPago");

            migrationBuilder.AddColumn<int>(
                name: "Frecuencia",
                table: "ConceptosPago",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Frecuencia",
                table: "ConceptosPago");

            migrationBuilder.AddColumn<bool>(
                name: "EsColegiatura",
                table: "ConceptosPago",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EsInscripcion",
                table: "ConceptosPago",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
