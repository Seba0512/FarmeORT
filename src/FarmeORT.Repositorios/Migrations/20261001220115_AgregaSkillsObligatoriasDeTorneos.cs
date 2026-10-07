using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmeORT.Repositorios.Migrations
{
    /// <inheritdoc />
    public partial class AgregaSkillsObligatoriasDeTorneos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SkillsObligatorias",
                columns: table => new
                {
                    TorneoId = table.Column<int>(type: "int", nullable: false),
                    SkillId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillsObligatorias", x => new { x.TorneoId, x.SkillId });
                    table.ForeignKey(
                        name: "FK_SkillsObligatorias_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SkillsObligatorias_Torneos_TorneoId",
                        column: x => x.TorneoId,
                        principalTable: "Torneos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SkillsObligatorias_SkillId",
                table: "SkillsObligatorias",
                column: "SkillId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SkillsObligatorias");
        }
    }
}
