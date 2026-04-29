using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gremelik.data.Migrations
{
    /// <inheritdoc />
    public partial class EstructuraCalificaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CalificacionesSEP",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AlumnoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GrupoId = table.Column<int>(type: "int", nullable: false),
                    MateriaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CicloEscolarId = table.Column<int>(type: "int", nullable: false),
                    Trimestre = table.Column<int>(type: "int", nullable: false),
                    PromedioSugerido = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    NotaFinal = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Confirmado = table.Column<bool>(type: "bit", nullable: false),
                    Usuario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FUM = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalificacionesSEP", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalificacionesSEP_Alumnos_AlumnoId",
                        column: x => x.AlumnoId,
                        principalTable: "Alumnos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CalificacionesSEP_CiclosEscolares_CicloEscolarId",
                        column: x => x.CicloEscolarId,
                        principalTable: "CiclosEscolares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CalificacionesSEP_Grupos_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "Grupos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CalificacionesSEP_Materias_MateriaId",
                        column: x => x.MateriaId,
                        principalTable: "Materias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracionesAcademicas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NivelEducativoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoPeriodoInterno = table.Column<int>(type: "int", nullable: false),
                    UsaDecimales = table.Column<bool>(type: "bit", nullable: false),
                    CalificacionMinima = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Usuario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FUM = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionesAcademicas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfiguracionesAcademicas_NivelesEducativos_NivelEducativoId",
                        column: x => x.NivelEducativoId,
                        principalTable: "NivelesEducativos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PeriodosInternos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CicloEscolarId = table.Column<int>(type: "int", nullable: false),
                    NivelEducativoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrimestreSEP = table.Column<int>(type: "int", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Usuario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FUM = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodosInternos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeriodosInternos_CiclosEscolares_CicloEscolarId",
                        column: x => x.CicloEscolarId,
                        principalTable: "CiclosEscolares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PeriodosInternos_NivelesEducativos_NivelEducativoId",
                        column: x => x.NivelEducativoId,
                        principalTable: "NivelesEducativos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CalificacionesInternas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AlumnoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GrupoId = table.Column<int>(type: "int", nullable: false),
                    MateriaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PeriodoInternoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nota = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Usuario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FUM = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalificacionesInternas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalificacionesInternas_Alumnos_AlumnoId",
                        column: x => x.AlumnoId,
                        principalTable: "Alumnos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CalificacionesInternas_Grupos_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "Grupos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CalificacionesInternas_Materias_MateriaId",
                        column: x => x.MateriaId,
                        principalTable: "Materias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CalificacionesInternas_PeriodosInternos_PeriodoInternoId",
                        column: x => x.PeriodoInternoId,
                        principalTable: "PeriodosInternos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CalificacionesInternas_AlumnoId",
                table: "CalificacionesInternas",
                column: "AlumnoId");

            migrationBuilder.CreateIndex(
                name: "IX_CalificacionesInternas_GrupoId",
                table: "CalificacionesInternas",
                column: "GrupoId");

            migrationBuilder.CreateIndex(
                name: "IX_CalificacionesInternas_MateriaId",
                table: "CalificacionesInternas",
                column: "MateriaId");

            migrationBuilder.CreateIndex(
                name: "IX_CalificacionesInternas_PeriodoInternoId",
                table: "CalificacionesInternas",
                column: "PeriodoInternoId");

            migrationBuilder.CreateIndex(
                name: "IX_CalificacionesSEP_AlumnoId",
                table: "CalificacionesSEP",
                column: "AlumnoId");

            migrationBuilder.CreateIndex(
                name: "IX_CalificacionesSEP_CicloEscolarId",
                table: "CalificacionesSEP",
                column: "CicloEscolarId");

            migrationBuilder.CreateIndex(
                name: "IX_CalificacionesSEP_GrupoId",
                table: "CalificacionesSEP",
                column: "GrupoId");

            migrationBuilder.CreateIndex(
                name: "IX_CalificacionesSEP_MateriaId",
                table: "CalificacionesSEP",
                column: "MateriaId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesAcademicas_NivelEducativoId",
                table: "ConfiguracionesAcademicas",
                column: "NivelEducativoId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodosInternos_CicloEscolarId",
                table: "PeriodosInternos",
                column: "CicloEscolarId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodosInternos_NivelEducativoId",
                table: "PeriodosInternos",
                column: "NivelEducativoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalificacionesInternas");

            migrationBuilder.DropTable(
                name: "CalificacionesSEP");

            migrationBuilder.DropTable(
                name: "ConfiguracionesAcademicas");

            migrationBuilder.DropTable(
                name: "PeriodosInternos");
        }
    }
}
