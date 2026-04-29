using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gremelik.data.Migrations
{
    /// <inheritdoc />
    public partial class AgregaReportesConducta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReportesConducta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CicloEscolarId = table.Column<int>(type: "int", nullable: false),
                    FechaIncidencia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AlumnoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportadoPorId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NombreReportador = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Gravedad = table.Column<int>(type: "int", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccionTomada = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Estatus = table.Column<int>(type: "int", nullable: false),
                    Usuario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FUM = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportesConducta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportesConducta_Alumnos_AlumnoId",
                        column: x => x.AlumnoId,
                        principalTable: "Alumnos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportesConducta_CiclosEscolares_CicloEscolarId",
                        column: x => x.CicloEscolarId,
                        principalTable: "CiclosEscolares",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReportesConducta_AlumnoId",
                table: "ReportesConducta",
                column: "AlumnoId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportesConducta_CicloEscolarId",
                table: "ReportesConducta",
                column: "CicloEscolarId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportesConducta");
        }
    }
}
