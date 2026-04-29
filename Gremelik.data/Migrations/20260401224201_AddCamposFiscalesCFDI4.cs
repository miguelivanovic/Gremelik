using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gremelik.data.Migrations
{
    /// <inheritdoc />
    public partial class AddCamposFiscalesCFDI4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UsoCFDI",
                table: "Tutores",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NivelSAT",
                table: "NivelesEducativos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ClaveProdServ",
                table: "ConceptosPago",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ClaveUnidad",
                table: "ConceptosPago",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "EsExentoIva",
                table: "ConceptosPago",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ObjetoImpuesto",
                table: "ConceptosPago",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UsoCFDI",
                table: "Tutores");

            migrationBuilder.DropColumn(
                name: "NivelSAT",
                table: "NivelesEducativos");

            migrationBuilder.DropColumn(
                name: "ClaveProdServ",
                table: "ConceptosPago");

            migrationBuilder.DropColumn(
                name: "ClaveUnidad",
                table: "ConceptosPago");

            migrationBuilder.DropColumn(
                name: "EsExentoIva",
                table: "ConceptosPago");

            migrationBuilder.DropColumn(
                name: "ObjetoImpuesto",
                table: "ConceptosPago");
        }
    }
}
