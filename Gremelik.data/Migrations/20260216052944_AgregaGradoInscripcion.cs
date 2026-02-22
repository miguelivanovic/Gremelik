using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gremelik.data.Migrations
{
    /// <inheritdoc />
    public partial class AgregaGradoInscripcion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GradoId",
                table: "Inscripciones",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inscripciones_GradoId",
                table: "Inscripciones",
                column: "GradoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Inscripciones_Grados_GradoId",
                table: "Inscripciones",
                column: "GradoId",
                principalTable: "Grados",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inscripciones_Grados_GradoId",
                table: "Inscripciones");

            migrationBuilder.DropIndex(
                name: "IX_Inscripciones_GradoId",
                table: "Inscripciones");

            migrationBuilder.DropColumn(
                name: "GradoId",
                table: "Inscripciones");
        }
    }
}
