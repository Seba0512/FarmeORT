using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmeORT.Repositorios.Migrations
{
    /// <inheritdoc />
    public partial class AgregaCampeonDeTorneos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CampeonId",
                table: "Torneos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Torneos_CampeonId",
                table: "Torneos",
                column: "CampeonId");

            migrationBuilder.AddForeignKey(
                name: "FK_Torneos_Usuarios_CampeonId",
                table: "Torneos",
                column: "CampeonId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Torneos_Usuarios_CampeonId",
                table: "Torneos");

            migrationBuilder.DropIndex(
                name: "IX_Torneos_CampeonId",
                table: "Torneos");

            migrationBuilder.DropColumn(
                name: "CampeonId",
                table: "Torneos");
        }
    }
}
