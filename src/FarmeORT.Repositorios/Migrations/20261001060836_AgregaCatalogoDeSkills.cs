using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FarmeORT.Repositorios.Migrations
{
    /// <inheritdoc />
    public partial class AgregaCatalogoDeSkills : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Skills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Dificultad = table.Column<int>(type: "int", nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skills", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Skills_Codigo",
                table: "Skills",
                column: "Codigo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Skills");
        }
    }
}
