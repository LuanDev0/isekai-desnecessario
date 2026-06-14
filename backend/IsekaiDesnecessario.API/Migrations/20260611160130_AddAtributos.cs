using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IsekaiDesnecessario.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAtributos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AtributoId",
                table: "MausHabitos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AtributoId",
                table: "BonsHabitos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Atributos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Emoji = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cor = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Atributos", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Atributos",
                columns: new[] { "Id", "Cor", "Descricao", "Emoji", "Nome" },
                values: new object[,]
                {
                    { 1, "#58a6ff", "Estudar, fazer cursos, resolver exercícios", "🧠", "Inteligência" },
                    { 2, "#bc8cff", "Ler livros, podcasts, reflexão", "📚", "Sabedoria" },
                    { 3, "#3fb950", "Treinar, dormir bem, beber água, dieta", "💪", "Físico" },
                    { 4, "#f78166", "Tarefas domésticas, rotina, pontualidade", "⚙️", "Disciplina" },
                    { 5, "#ffd700", "Pomodoro, sem celular, deep work", "🎯", "Foco" },
                    { 6, "#f85149", "Sono, hidratação, pausas, saúde geral", "❤️", "Vitalidade" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MausHabitos_AtributoId",
                table: "MausHabitos",
                column: "AtributoId");

            migrationBuilder.CreateIndex(
                name: "IX_BonsHabitos_AtributoId",
                table: "BonsHabitos",
                column: "AtributoId");

            migrationBuilder.AddForeignKey(
                name: "FK_BonsHabitos_Atributos_AtributoId",
                table: "BonsHabitos",
                column: "AtributoId",
                principalTable: "Atributos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MausHabitos_Atributos_AtributoId",
                table: "MausHabitos",
                column: "AtributoId",
                principalTable: "Atributos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BonsHabitos_Atributos_AtributoId",
                table: "BonsHabitos");

            migrationBuilder.DropForeignKey(
                name: "FK_MausHabitos_Atributos_AtributoId",
                table: "MausHabitos");

            migrationBuilder.DropTable(
                name: "Atributos");

            migrationBuilder.DropIndex(
                name: "IX_MausHabitos_AtributoId",
                table: "MausHabitos");

            migrationBuilder.DropIndex(
                name: "IX_BonsHabitos_AtributoId",
                table: "BonsHabitos");

            migrationBuilder.DropColumn(
                name: "AtributoId",
                table: "MausHabitos");

            migrationBuilder.DropColumn(
                name: "AtributoId",
                table: "BonsHabitos");
        }
    }
}
