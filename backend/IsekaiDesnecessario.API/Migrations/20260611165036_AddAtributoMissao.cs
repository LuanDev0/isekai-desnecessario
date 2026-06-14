using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IsekaiDesnecessario.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAtributoMissao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AtributoId",
                table: "Missoes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Missoes_AtributoId",
                table: "Missoes",
                column: "AtributoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Missoes_Atributos_AtributoId",
                table: "Missoes",
                column: "AtributoId",
                principalTable: "Atributos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Missoes_Atributos_AtributoId",
                table: "Missoes");

            migrationBuilder.DropIndex(
                name: "IX_Missoes_AtributoId",
                table: "Missoes");

            migrationBuilder.DropColumn(
                name: "AtributoId",
                table: "Missoes");
        }
    }
}
