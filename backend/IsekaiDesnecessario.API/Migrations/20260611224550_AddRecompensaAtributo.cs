using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IsekaiDesnecessario.API.Migrations
{
    /// <inheritdoc />
    public partial class AddRecompensaAtributo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AtributoId",
                table: "Recompensas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PontosNecessarios",
                table: "Recompensas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Atributos",
                keyColumn: "Id",
                keyValue: 3,
                column: "Descricao",
                value: "Treinar, academia, exercícios físicos");

            migrationBuilder.UpdateData(
                table: "Atributos",
                keyColumn: "Id",
                keyValue: 6,
                column: "Descricao",
                value: "Sono, hidratação, dieta, pausas");

            migrationBuilder.CreateIndex(
                name: "IX_Recompensas_AtributoId",
                table: "Recompensas",
                column: "AtributoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Recompensas_Atributos_AtributoId",
                table: "Recompensas",
                column: "AtributoId",
                principalTable: "Atributos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recompensas_Atributos_AtributoId",
                table: "Recompensas");

            migrationBuilder.DropIndex(
                name: "IX_Recompensas_AtributoId",
                table: "Recompensas");

            migrationBuilder.DropColumn(
                name: "AtributoId",
                table: "Recompensas");

            migrationBuilder.DropColumn(
                name: "PontosNecessarios",
                table: "Recompensas");

            migrationBuilder.UpdateData(
                table: "Atributos",
                keyColumn: "Id",
                keyValue: 3,
                column: "Descricao",
                value: "Treinar, dormir bem, beber água, dieta");

            migrationBuilder.UpdateData(
                table: "Atributos",
                keyColumn: "Id",
                keyValue: 6,
                column: "Descricao",
                value: "Sono, hidratação, pausas, saúde geral");
        }
    }
}
