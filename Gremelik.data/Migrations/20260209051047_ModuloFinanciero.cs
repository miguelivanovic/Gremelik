using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gremelik.data.Migrations
{
    /// <inheritdoc />
    public partial class ModuloFinanciero : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BecaPorcentaje",
                table: "Inscripciones",
                newName: "MontoFinal");

            migrationBuilder.AddColumn<decimal>(
                name: "MontoBase",
                table: "Inscripciones",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoDescuento",
                table: "Inscripciones",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "MotivoDescuentoManual",
                table: "Inscripciones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReglaDescuentoId",
                table: "Inscripciones",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CostosInscripcion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CicloEscolarId = table.Column<int>(type: "int", nullable: false),
                    NivelEducativoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GradoId = table.Column<int>(type: "int", nullable: true),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Concepto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Usuario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FUM = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CostosInscripcion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CostosInscripcion_CiclosEscolares_CicloEscolarId",
                        column: x => x.CicloEscolarId,
                        principalTable: "CiclosEscolares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CostosInscripcion_Grados_GradoId",
                        column: x => x.GradoId,
                        principalTable: "Grados",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CostosInscripcion_NivelesEducativos_NivelEducativoId",
                        column: x => x.NivelEducativoId,
                        principalTable: "NivelesEducativos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReglasDescuento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Porcentaje = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MontoFijo = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FechaInicioValidez = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaFinValidez = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CicloEscolarId = table.Column<int>(type: "int", nullable: false),
                    EscuelaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Usuario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FUM = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReglasDescuento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReglasDescuento_CiclosEscolares_CicloEscolarId",
                        column: x => x.CicloEscolarId,
                        principalTable: "CiclosEscolares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inscripciones_ReglaDescuentoId",
                table: "Inscripciones",
                column: "ReglaDescuentoId");

            migrationBuilder.CreateIndex(
                name: "IX_CostosInscripcion_CicloEscolarId",
                table: "CostosInscripcion",
                column: "CicloEscolarId");

            migrationBuilder.CreateIndex(
                name: "IX_CostosInscripcion_GradoId",
                table: "CostosInscripcion",
                column: "GradoId");

            migrationBuilder.CreateIndex(
                name: "IX_CostosInscripcion_NivelEducativoId",
                table: "CostosInscripcion",
                column: "NivelEducativoId");

            migrationBuilder.CreateIndex(
                name: "IX_ReglasDescuento_CicloEscolarId",
                table: "ReglasDescuento",
                column: "CicloEscolarId");

            migrationBuilder.AddForeignKey(
                name: "FK_Inscripciones_ReglasDescuento_ReglaDescuentoId",
                table: "Inscripciones",
                column: "ReglaDescuentoId",
                principalTable: "ReglasDescuento",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inscripciones_ReglasDescuento_ReglaDescuentoId",
                table: "Inscripciones");

            migrationBuilder.DropTable(
                name: "CostosInscripcion");

            migrationBuilder.DropTable(
                name: "ReglasDescuento");

            migrationBuilder.DropIndex(
                name: "IX_Inscripciones_ReglaDescuentoId",
                table: "Inscripciones");

            migrationBuilder.DropColumn(
                name: "MontoBase",
                table: "Inscripciones");

            migrationBuilder.DropColumn(
                name: "MontoDescuento",
                table: "Inscripciones");

            migrationBuilder.DropColumn(
                name: "MotivoDescuentoManual",
                table: "Inscripciones");

            migrationBuilder.DropColumn(
                name: "ReglaDescuentoId",
                table: "Inscripciones");

            migrationBuilder.RenameColumn(
                name: "MontoFinal",
                table: "Inscripciones",
                newName: "BecaPorcentaje");
        }
    }
}
