using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gremelik.data.Migrations
{
    /// <inheritdoc />
    public partial class ModuloFinanciero_ConceptosPlanes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConceptosPago",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MontoDefault = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EsColegiatura = table.Column<bool>(type: "bit", nullable: false),
                    EsInscripcion = table.Column<bool>(type: "bit", nullable: false),
                    AplicaBeca = table.Column<bool>(type: "bit", nullable: false),
                    Obligatorio = table.Column<bool>(type: "bit", nullable: false),
                    CicloEscolarId = table.Column<int>(type: "int", nullable: false),
                    Usuario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FUM = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConceptosPago", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlanesPago",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NumeroPagos = table.Column<int>(type: "int", nullable: false),
                    DiaLimitePago = table.Column<int>(type: "int", nullable: false),
                    MesesDobleCobro = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RecargoMonto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RecargoPorcentaje = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CicloEscolarId = table.Column<int>(type: "int", nullable: false),
                    ConceptoPagoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Usuario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FUM = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanesPago", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanesPago_ConceptosPago_ConceptoPagoId",
                        column: x => x.ConceptoPagoId,
                        principalTable: "ConceptosPago",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlanesPago_ConceptoPagoId",
                table: "PlanesPago",
                column: "ConceptoPagoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlanesPago");

            migrationBuilder.DropTable(
                name: "ConceptosPago");
        }
    }
}
