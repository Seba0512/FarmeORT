using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmeORT.Repositorios.Migrations
{
    /// <inheritdoc />
    public partial class AgregaInscripcionesYBatallas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Batallas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TorneoId = table.Column<int>(type: "int", nullable: false),
                    NumeroRonda = table.Column<int>(type: "int", nullable: false),
                    Jugador1Id = table.Column<int>(type: "int", nullable: false),
                    Jugador2Id = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    JugadorInicialId = table.Column<int>(type: "int", nullable: false),
                    JugadorTurnoId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Batallas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Batallas_Torneos_TorneoId",
                        column: x => x.TorneoId,
                        principalTable: "Torneos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Batallas_Usuarios_Jugador1Id",
                        column: x => x.Jugador1Id,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Batallas_Usuarios_Jugador2Id",
                        column: x => x.Jugador2Id,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Inscripciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TorneoId = table.Column<int>(type: "int", nullable: false),
                    JugadorId = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inscripciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inscripciones_Torneos_TorneoId",
                        column: x => x.TorneoId,
                        principalTable: "Torneos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Inscripciones_Usuarios_JugadorId",
                        column: x => x.JugadorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Lanzamientos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BatallaId = table.Column<int>(type: "int", nullable: false),
                    Turno = table.Column<int>(type: "int", nullable: false),
                    JugadorId = table.Column<int>(type: "int", nullable: false),
                    SkillId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lanzamientos", x => x.Id);
                    table.CheckConstraint("CK_Lanzamientos_Turno", "[Turno] BETWEEN 1 AND 8");
                    table.ForeignKey(
                        name: "FK_Lanzamientos_Batallas_BatallaId",
                        column: x => x.BatallaId,
                        principalTable: "Batallas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Lanzamientos_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Lanzamientos_Usuarios_JugadorId",
                        column: x => x.JugadorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Batallas_Jugador1Id",
                table: "Batallas",
                column: "Jugador1Id");

            migrationBuilder.CreateIndex(
                name: "IX_Batallas_Jugador2Id",
                table: "Batallas",
                column: "Jugador2Id");

            migrationBuilder.CreateIndex(
                name: "IX_Batallas_TorneoId",
                table: "Batallas",
                column: "TorneoId");

            migrationBuilder.CreateIndex(
                name: "IX_Inscripciones_JugadorId",
                table: "Inscripciones",
                column: "JugadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Inscripciones_TorneoId_JugadorId",
                table: "Inscripciones",
                columns: new[] { "TorneoId", "JugadorId" });

            migrationBuilder.CreateIndex(
                name: "IX_Lanzamientos_BatallaId_JugadorId_SkillId",
                table: "Lanzamientos",
                columns: new[] { "BatallaId", "JugadorId", "SkillId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lanzamientos_BatallaId_Turno",
                table: "Lanzamientos",
                columns: new[] { "BatallaId", "Turno" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lanzamientos_JugadorId",
                table: "Lanzamientos",
                column: "JugadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Lanzamientos_SkillId",
                table: "Lanzamientos",
                column: "SkillId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Inscripciones");

            migrationBuilder.DropTable(
                name: "Lanzamientos");

            migrationBuilder.DropTable(
                name: "Batallas");
        }
    }
}
