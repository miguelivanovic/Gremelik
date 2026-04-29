using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gremelik.data.Migrations
{
    /// <inheritdoc />
    public partial class AgregaCicloAAsistencias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CicloEscolarId",
                table: "Asistencias",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Asistencias_CicloEscolarId",
                table: "Asistencias",
                column: "CicloEscolarId");

            migrationBuilder.AddForeignKey(
                name: "FK_Asistencias_CiclosEscolares_CicloEscolarId",
                table: "Asistencias",
                column: "CicloEscolarId",
                principalTable: "CiclosEscolares",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Asistencias_CiclosEscolares_CicloEscolarId",
                table: "Asistencias");

            migrationBuilder.DropIndex(
                name: "IX_Asistencias_CicloEscolarId",
                table: "Asistencias");

            migrationBuilder.DropColumn(
                name: "CicloEscolarId",
                table: "Asistencias");
        }
    }
}
