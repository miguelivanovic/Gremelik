using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gremelik.data.Migrations
{
    /// <inheritdoc />
    public partial class InicialSQL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Alumnos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PrimerApellido = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SegundoApellido = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Matricula = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CURP = table.Column<string>(type: "nvarchar(18)", maxLength: 18, nullable: false),
                    NIA = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FechaNacimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EscuelaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Usuario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FUM = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alumnos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Escuelas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RazonSocial = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CCT = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RVOE = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Usuario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FUM = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Escuelas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FichasMedicas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoSangre = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Alergias = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NombreContactoEmergencia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TelefonoContactoEmergencia = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AlumnoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Usuario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FUM = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FichasMedicas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RelacionAlumnoTutor",
                columns: table => new
                {
                    AlumnoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TutorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Parentesco = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Usuario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FUM = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelacionAlumnoTutor", x => new { x.AlumnoId, x.TutorId });
                });

            migrationBuilder.CreateTable(
                name: "Tutores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PrimerApellido = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SegundoApellido = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RFC = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    RegimenFiscal = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CodigoPostal = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    Usuario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FUM = table.Column<DateTime>(type: "datetime2", nullable: false)
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
