using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmeORT.Repositorios.Migrations
{
    /// <inheritdoc />
    public partial class AgregaRepertorioDeJugadores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SkillsDeclaradas",
                columns: table => new
                {
                    JugadorId = table.Column<int>(type: "int", nullable: false),
                    SkillId = table.Column<int>(type: "int", nullable: false),
                    Nivel = table.Column<int>(type: "int", nullable: false),
                    FechaDeclaracion = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillsDeclaradas", x => new { x.JugadorId, x.SkillId });
                    table.ForeignKey(
                        name: "FK_SkillsDeclaradas_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SkillsDeclaradas_Usuarios_JugadorId",
                        column: x => x.JugadorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SkillsDeclaradas_SkillId",
                table: "SkillsDeclaradas",
                column: "SkillId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SkillsDeclaradas");
        }
    }
}
