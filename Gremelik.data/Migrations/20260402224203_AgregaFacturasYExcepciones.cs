using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gremelik.data.Migrations
{
    /// <inheritdoc />
    public partial class AgregaFacturasYExcepciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Facturas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PagoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TutorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaEmision = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MetodoPagoSAT = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FormaPagoSAT = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Estatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    XmlCrudo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Uuid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    XmlTimbrado = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Usuario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FUM = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Facturas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Facturas_Pagos_PagoId",
                        column: x => x.PagoId,
                        principalTable: "Pagos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Facturas_Tutores_TutorId",
                        column: x => x.TutorId,
                        principalTable: "Tutores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExcepcionesCaja_CuentaPorCobrarId",
                table: "ExcepcionesCaja",
                column: "CuentaPorCobrarId");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_PagoId",
                table: "Facturas",
                column: "PagoId");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_TutorId",
                table: "Facturas",
                column: "TutorId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExcepcionesCaja_CuentasPorCobrar_CuentaPorCobrarId",
                table: "ExcepcionesCaja",
                column: "CuentaPorCobrarId",
                principalTable: "CuentasPorCobrar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExcepcionesCaja_CuentasPorCobrar_CuentaPorCobrarId",
                table: "ExcepcionesCaja");

            migrationBuilder.DropTable(
                name: "Facturas");

            migrationBuilder.DropIndex(
                name: "IX_ExcepcionesCaja_CuentaPorCobrarId",
                table: "ExcepcionesCaja");
        }
    }
}
