using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gremelik.data.Migrations
{
    /// <inheritdoc />
    public partial class Pago_FacturacionTutor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequiereFactura",
                table: "Pagos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "TutorId",
                table: "Pagos",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_TutorId",
                table: "Pagos",
                column: "TutorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pagos_Tutores_TutorId",
                table: "Pagos",
                column: "TutorId",
                principalTable: "Tutores",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pagos_Tutores_TutorId",
                table: "Pagos");

            migrationBuilder.DropIndex(
                name: "IX_Pagos_TutorId",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "RequiereFactura",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "TutorId",
                table: "Pagos");
        }
    }
}
