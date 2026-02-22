using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gremelik.data.Migrations
{
    /// <inheritdoc />
    public partial class TablaAlumnosGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CURP",
                table: "Alumnos",
                type: "nvarchar(18)",
                maxLength: 18,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_Alumnos_EscuelaId",
                table: "Alumnos",
                column: "EscuelaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Alumnos_Escuelas_EscuelaId",
                table: "Alumnos",
                column: "EscuelaId",
                principalTable: "Escuelas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alumnos_Escuelas_EscuelaId",
                table: "Alumnos");

            migrationBuilder.DropIndex(
                name: "IX_Alumnos_EscuelaId",
                table: "Alumnos");

            migrationBuilder.AlterColumn<string>(
                name: "CURP",
                table: "Alumnos",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(18)",
                oldMaxLength: 18);
        }
    }
}
