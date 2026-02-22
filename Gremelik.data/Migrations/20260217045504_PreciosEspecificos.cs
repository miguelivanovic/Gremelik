using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gremelik.data.Migrations
{
    /// <inheritdoc />
    public partial class PreciosEspecificos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConceptosPrecios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ConceptoPagoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlantelId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NivelEducativoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GradoId = table.Column<int>(type: "int", nullable: true),
                    Usuario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FUM = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConceptosPrecios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConceptosPrecios_ConceptosPago_ConceptoPagoId",
                        column: x => x.ConceptoPagoId,
                        principalTable: "ConceptosPago",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConceptosPrecios_Grados_GradoId",
                        column: x => x.GradoId,
                        principalTable: "Grados",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ConceptosPrecios_NivelesEducativos_NivelEducativoId",
                        column: x => x.NivelEducativoId,
                        principalTable: "NivelesEducativos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ConceptosPrecios_Planteles_PlantelId",
                        column: x => x.PlantelId,
                        principalTable: "Planteles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConceptosPrecios_ConceptoPagoId",
                table: "ConceptosPrecios",
                column: "ConceptoPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_ConceptosPrecios_GradoId",
                table: "ConceptosPrecios",
                column: "GradoId");

            migrationBuilder.CreateIndex(
                name: "IX_ConceptosPrecios_NivelEducativoId",
                table: "ConceptosPrecios",
                column: "NivelEducativoId");

            migrationBuilder.CreateIndex(
                name: "IX_ConceptosPrecios_PlantelId",
                table: "ConceptosPrecios",
                column: "PlantelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConceptosPrecios");
        }
    }
}
