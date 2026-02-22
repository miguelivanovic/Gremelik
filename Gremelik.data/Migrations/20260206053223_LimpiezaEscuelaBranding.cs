using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gremelik.data.Migrations
{
    /// <inheritdoc />
    public partial class LimpiezaEscuelaBranding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CCT",
                table: "Escuelas");

            migrationBuilder.DropColumn(
                name: "RVOE",
                table: "Escuelas");

            migrationBuilder.DropColumn(
                name: "RazonSocial",
                table: "Escuelas");

            migrationBuilder.AddColumn<string>(
                name: "ColorPrimario",
                table: "Escuelas",
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ColorSecundario",
                table: "Escuelas",
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FondoLoginUrl",
                table: "Escuelas",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "Escuelas",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SitioWeb",
                table: "Escuelas",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slogan",
                table: "Escuelas",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColorPrimario",
                table: "Escuelas");

            migrationBuilder.DropColumn(
                name: "ColorSecundario",
                table: "Escuelas");

            migrationBuilder.DropColumn(
                name: "FondoLoginUrl",
                table: "Escuelas");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "Escuelas");

            migrationBuilder.DropColumn(
                name: "SitioWeb",
                table: "Escuelas");

            migrationBuilder.DropColumn(
                name: "Slogan",
                table: "Escuelas");

            migrationBuilder.AddColumn<string>(
                name: "CCT",
                table: "Escuelas",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RVOE",
                table: "Escuelas",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RazonSocial",
                table: "Escuelas",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
