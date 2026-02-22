using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gremelik.data.Migrations
{
    /// <inheritdoc />
    public partial class SimplificacionFinalFinanzas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConceptosPrecios");

            migrationBuilder.RenameColumn(
                name: "MontoDefault",
                table: "ConceptosPago",
                newName: "Monto");

            migrationBuilder.AddColumn<int>(
                name: "GradoId",
                table: "ConceptosPago",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "NivelEducativoId",
                table: "ConceptosPago",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreGrado",
                table: "ConceptosPago",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreNivel",
                table: "ConceptosPago",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombrePlantel",
                table: "ConceptosPago",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PlantelId",
                table: "ConceptosPago",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GradoId",
                table: "ConceptosPago");

            migrationBuilder.DropColumn(
                name: "NivelEducativoId",
                table: "ConceptosPago");

            migrationBuilder.DropColumn(
                name: "NombreGrado",
                table: "ConceptosPago");

            migrationBuilder.DropColumn(
                name: "NombreNivel",
                table: "ConceptosPago");

            migrationBuilder.DropColumn(
                name: "NombrePlantel",
                table: "ConceptosPago");

            migrationBuilder.DropColumn(
                name: "PlantelId",
                table: "ConceptosPago");

            migrationBuilder.RenameColumn(
                name: "Monto",
                table: "ConceptosPago",
                newName: "MontoDefault");

            migrationBuilder.CreateTable(
                name: "ConceptosPrecios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConceptoPagoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GradoId = table.Column<int>(type: "int", nullable: true),
                    NivelEducativoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PlantelId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FUM = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Usuario = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
    }
}
