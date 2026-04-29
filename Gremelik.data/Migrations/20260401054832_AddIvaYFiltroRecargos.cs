using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gremelik.data.Migrations
{
    /// <inheritdoc />
    public partial class AddIvaYFiltroRecargos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AplicaIva",
                table: "ConfiguracionesRecargo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IvaIncluido",
                table: "ConfiguracionesRecargo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "GeneraRecargos",
                table: "ConceptosPago",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AplicaIva",
                table: "ConfiguracionesRecargo");

            migrationBuilder.DropColumn(
                name: "IvaIncluido",
                table: "ConfiguracionesRecargo");

            migrationBuilder.DropColumn(
                name: "GeneraRecargos",
                table: "ConceptosPago");
        }
    }
}
