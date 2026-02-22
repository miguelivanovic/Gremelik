using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gremelik.data.Migrations
{
    /// <inheritdoc />
    public partial class CambioEstatusCiclos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Actual",
                table: "CiclosEscolares",
                newName: "Activo");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "CiclosEscolares",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "Estatus",
                table: "CiclosEscolares",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "FUM",
                table: "CiclosEscolares",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaRegistro",
                table: "CiclosEscolares",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Usuario",
                table: "CiclosEscolares",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Estatus",
                table: "CiclosEscolares");

            migrationBuilder.DropColumn(
                name: "FUM",
                table: "CiclosEscolares");

            migrationBuilder.DropColumn(
                name: "FechaRegistro",
                table: "CiclosEscolares");

            migrationBuilder.DropColumn(
                name: "Usuario",
                table: "CiclosEscolares");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "CiclosEscolares",
                newName: "Actual");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "CiclosEscolares",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);
        }
    }
}
