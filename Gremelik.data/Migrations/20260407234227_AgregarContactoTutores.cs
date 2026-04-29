using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gremelik.data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarContactoTutores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CorreoElectronico",
                table: "Tutores",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DireccionFisica",
                table: "Tutores",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TelefonoMovil",
                table: "Tutores",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CorreoElectronico",
                table: "Tutores");

            migrationBuilder.DropColumn(
                name: "DireccionFisica",
                table: "Tutores");

            migrationBuilder.DropColumn(
                name: "TelefonoMovil",
                table: "Tutores");
        }
    }
}
