using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IsekaiDesnecessario.API.Migrations
{
    /// <inheritdoc />
    public partial class RenameDesafioConcluidoEm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DesafioConcluídoEm",
                table: "Perfis",
                newName: "DesafioConcluidoEm");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DesafioConcluidoEm",
                table: "Perfis",
                newName: "DesafioConcluídoEm");
        }
    }
}
