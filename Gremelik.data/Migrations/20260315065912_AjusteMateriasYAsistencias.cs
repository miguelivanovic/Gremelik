using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gremelik.data.Migrations
{
    /// <inheritdoc />
    public partial class AjusteMateriasYAsistencias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Asistencias_Grupos_GrupoId",
                table: "Asistencias");

            migrationBuilder.AddColumn<Guid>(
                name: "MateriaId",
                table: "Asistencias",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Materias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlantelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GradoId = table.Column<int>(type: "int", nullable: true),
                    GrupoId = table.Column<int>(type: "int", nullable: true),
                    Usuario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FUM = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Materias_Grados_GradoId",
                        column: x => x.GradoId,
                        principalTable: "Grados",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Materias_Grupos_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "Grupos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Materias_Planteles_PlantelId",
                        column: x => x.PlantelId,
                        principalTable: "Planteles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AsignacionesMaestros",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaestroId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MateriaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GrupoId = table.Column<int>(type: "int", nullable: false),
                    CicloEscolarId = table.Column<int>(type: "int", nullable: false),
                    Usuario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FUM = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsignacionesMaestros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AsignacionesMaestros_CiclosEscolares_CicloEscolarId",
                        column: x => x.CicloEscolarId,
                        principalTable: "CiclosEscolares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AsignacionesMaestros_Grupos_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "Grupos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AsignacionesMaestros_Materias_MateriaId",
                        column: x => x.MateriaId,
                        principalTable: "Materias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Asistencias_MateriaId",
                table: "Asistencias",
                column: "MateriaId");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesMaestros_CicloEscolarId",
                table: "AsignacionesMaestros",
                column: "CicloEscolarId");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesMaestros_GrupoId",
                table: "AsignacionesMaestros",
                column: "GrupoId");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesMaestros_MateriaId",
                table: "AsignacionesMaestros",
                column: "MateriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Materias_GradoId",
                table: "Materias",
                column: "GradoId");

            migrationBuilder.CreateIndex(
                name: "IX_Materias_GrupoId",
                table: "Materias",
                column: "GrupoId");

            migrationBuilder.CreateIndex(
                name: "IX_Materias_PlantelId",
                table: "Materias",
                column: "PlantelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Asistencias_Grupos_GrupoId",
                table: "Asistencias",
                column: "GrupoId",
                principalTable: "Grupos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Asistencias_Materias_MateriaId",
                table: "Asistencias",
                column: "MateriaId",
                principalTable: "Materias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Asistencias_Grupos_GrupoId",
                table: "Asistencias");

            migrationBuilder.DropForeignKey(
                name: "FK_Asistencias_Materias_MateriaId",
                table: "Asistencias");

            migrationBuilder.DropTable(
                name: "AsignacionesMaestros");

            migrationBuilder.DropTable(
                name: "Materias");

            migrationBuilder.DropIndex(
                name: "IX_Asistencias_MateriaId",
                table: "Asistencias");

            migrationBuilder.DropColumn(
                name: "MateriaId",
                table: "Asistencias");

            migrationBuilder.AddForeignKey(
                name: "FK_Asistencias_Grupos_GrupoId",
                table: "Asistencias",
                column: "GrupoId",
                principalTable: "Grupos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
