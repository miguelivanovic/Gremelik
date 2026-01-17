using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gremelik.data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Alumnos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PrimerApellido = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SegundoApellido = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Matricula = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CURP = table.Column<string>(type: "TEXT", maxLength: 18, nullable: false),
                    NIA = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    FechaNacimiento = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EscuelaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Usuario = table.Column<string>(type: "TEXT", nullable: false),
                    FUM = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alumnos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Escuelas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RazonSocial = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CCT = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    RVOE = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Usuario = table.Column<string>(type: "TEXT", nullable: false),
                    FUM = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Escuelas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FichasMedicas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TipoSangre = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Alergias = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    NombreContactoEmergencia = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TelefonoContactoEmergencia = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    AlumnoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Usuario = table.Column<string>(type: "TEXT", nullable: false),
                    FUM = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FichasMedicas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RelacionAlumnoTutor",
                columns: table => new
                {
                    AlumnoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TutorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Parentesco = table.Column<string>(type: "TEXT", nullable: false),
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Usuario = table.Column<string>(type: "TEXT", nullable: false),
                    FUM = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelacionAlumnoTutor", x => new { x.AlumnoId, x.TutorId });
                });

            migrationBuilder.CreateTable(
                name: "Tutores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PrimerApellido = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SegundoApellido = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    RFC = table.Column<string>(type: "TEXT", maxLength: 13, nullable: false),
                    RegimenFiscal = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CodigoPostal = table.Column<string>(type: "TEXT", maxLength: 5, nullable: false),
                    Usuario = table.Column<string>(type: "TEXT", nullable: false),
                    FUM = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tutores", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alumnos");

            migrationBuilder.DropTable(
                name: "Escuelas");

            migrationBuilder.DropTable(
                name: "FichasMedicas");

            migrationBuilder.DropTable(
                name: "RelacionAlumnoTutor");

            migrationBuilder.DropTable(
                name: "Tutores");
        }
    }
}
