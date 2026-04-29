using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gremelik.data.Migrations
{
    /// <inheritdoc />
    public partial class AgregaMateriaAObservacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Observaciones_Grupos_GrupoId",
                table: "Observaciones");

            migrationBuilder.AddColumn<Guid>(
                name: "MateriaId",
                table: "Observaciones",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Observaciones_MateriaId",
                table: "Observaciones",
                column: "MateriaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Observaciones_Grupos_GrupoId",
                table: "Observaciones",
                column: "GrupoId",
                principalTable: "Grupos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Observaciones_Materias_MateriaId",
                table: "Observaciones",
                column: "MateriaId",
                principalTable: "Materias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Observaciones_Grupos_GrupoId",
                table: "Observaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_Observaciones_Materias_MateriaId",
                table: "Observaciones");

            migrationBuilder.DropIndex(
                name: "IX_Observaciones_MateriaId",
                table: "Observaciones");

            migrationBuilder.DropColumn(
                name: "MateriaId",
                table: "Observaciones");

            migrationBuilder.AddForeignKey(
                name: "FK_Observaciones_Grupos_GrupoId",
                table: "Observaciones",
                column: "GrupoId",
                principalTable: "Grupos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
