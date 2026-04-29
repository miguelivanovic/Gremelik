using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gremelik.data.Migrations
{
    /// <inheritdoc />
    public partial class AddCajaRecargosExcepciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Autorizacion",
                table: "Pagos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Banco",
                table: "Pagos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TerminacionTarjeta",
                table: "Pagos",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ConfiguracionesRecargo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NombreConcepto = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DiasGracia = table.Column<int>(type: "int", nullable: false),
                    MontoFijo = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Porcentaje = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CicloEscolarId = table.Column<int>(type: "int", nullable: false),
                    EscuelaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Usuario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FUM = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionesRecargo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExcepcionesCaja",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PagoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CuentaPorCobrarId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    BecaRestauradaMonto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RecargoPerdonadoMonto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EscuelaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Usuario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FUM = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExcepcionesCaja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExcepcionesCaja_Pagos_PagoId",
                        column: x => x.PagoId,
                        principalTable: "Pagos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExcepcionesCaja_PagoId",
                table: "ExcepcionesCaja",
                column: "PagoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracionesRecargo");

            migrationBuilder.DropTable(
                name: "ExcepcionesCaja");

            migrationBuilder.DropColumn(
                name: "Autorizacion",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "Banco",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "TerminacionTarjeta",
                table: "Pagos");
        }
    }
}
