using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gremelik.data.Migrations
{
    /// <inheritdoc />
    public partial class AddTransaccionBancaria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TransaccionesBancarias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EscuelaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Banco = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReferenciaBancaria = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FechaPago = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClaveRastreo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Estatus = table.Column<int>(type: "int", nullable: false),
                    AlumnoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PagoGeneradoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Usuario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FUM = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransaccionesBancarias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransaccionesBancarias_Alumnos_AlumnoId",
                        column: x => x.AlumnoId,
                        principalTable: "Alumnos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TransaccionesBancarias_Escuelas_EscuelaId",
                        column: x => x.EscuelaId,
                        principalTable: "Escuelas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransaccionesBancarias_Pagos_PagoGeneradoId",
                        column: x => x.PagoGeneradoId,
                        principalTable: "Pagos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TransaccionesBancarias_AlumnoId",
                table: "TransaccionesBancarias",
                column: "AlumnoId");

            migrationBuilder.CreateIndex(
                name: "IX_TransaccionesBancarias_EscuelaId",
                table: "TransaccionesBancarias",
                column: "EscuelaId");

            migrationBuilder.CreateIndex(
                name: "IX_TransaccionesBancarias_PagoGeneradoId",
                table: "TransaccionesBancarias",
                column: "PagoGeneradoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransaccionesBancarias");
        }
    }
}
