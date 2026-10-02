using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmeORT.Repositorios.Migrations
{
    /// <inheritdoc />
    public partial class AgregaFallosDeBatallas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CambioEloJugador1",
                table: "Batallas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CambioEloJugador2",
                table: "Batallas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Comentario",
                table: "Batallas",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GanadorId",
                table: "Batallas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Batallas_GanadorId",
                table: "Batallas",
                column: "GanadorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Batallas_Usuarios_GanadorId",
                table: "Batallas",
                column: "GanadorId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Batallas_Usuarios_GanadorId",
                table: "Batallas");

            migrationBuilder.DropIndex(
                name: "IX_Batallas_GanadorId",
                table: "Batallas");

            migrationBuilder.DropColumn(
                name: "CambioEloJugador1",
                table: "Batallas");

            migrationBuilder.DropColumn(
                name: "CambioEloJugador2",
                table: "Batallas");

            migrationBuilder.DropColumn(
                name: "Comentario",
                table: "Batallas");

            migrationBuilder.DropColumn(
                name: "GanadorId",
                table: "Batallas");
        }
    }
}
