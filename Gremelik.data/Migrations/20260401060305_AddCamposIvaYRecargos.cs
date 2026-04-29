using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gremelik.data.Migrations
{
    /// <inheritdoc />
    public partial class AddCamposIvaYRecargos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_CuentasPorCobrar_ConceptoPagoId",
                table: "CuentasPorCobrar",
                column: "ConceptoPagoId");

            migrationBuilder.AddForeignKey(
                name: "FK_CuentasPorCobrar_ConceptosPago_ConceptoPagoId",
                table: "CuentasPorCobrar",
                column: "ConceptoPagoId",
                principalTable: "ConceptosPago",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CuentasPorCobrar_ConceptosPago_ConceptoPagoId",
                table: "CuentasPorCobrar");

            migrationBuilder.DropIndex(
                name: "IX_CuentasPorCobrar_ConceptoPagoId",
                table: "CuentasPorCobrar");
        }
    }
}
