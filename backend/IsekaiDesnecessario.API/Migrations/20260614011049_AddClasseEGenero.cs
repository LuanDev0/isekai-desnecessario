using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IsekaiDesnecessario.API.Migrations
{
    /// <inheritdoc />
    public partial class AddClasseEGenero : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClasseId",
                table: "Perfis",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Genero",
                table: "Perfis",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Classes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Emoji = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AtributoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Classes_Atributos_AtributoId",
                        column: x => x.AtributoId,
                        principalTable: "Atributos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Classes",
                columns: new[] { "Id", "AtributoId", "Emoji", "Nome" },
                values: new object[,]
                {
                    { 1, 1, "🧙", "Mago" },
                    { 2, 2, "🎵", "Bardo" },
                    { 3, 3, "⚔️", "Guerreiro" },
                    { 4, 4, "🛡️", "Escudeiro" },
                    { 5, 5, "🎯", "Executor" },
                    { 6, 6, "✨", "Clérigo" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Perfis_ClasseId",
                table: "Perfis",
                column: "ClasseId");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_AtributoId",
                table: "Classes",
                column: "AtributoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Perfis_Classes_ClasseId",
                table: "Perfis",
                column: "ClasseId",
                principalTable: "Classes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Perfis_Classes_ClasseId",
                table: "Perfis");

            migrationBuilder.DropTable(
                name: "Classes");

            migrationBuilder.DropIndex(
                name: "IX_Perfis_ClasseId",
                table: "Perfis");

            migrationBuilder.DropColumn(
                name: "ClasseId",
                table: "Perfis");

            migrationBuilder.DropColumn(
                name: "Genero",
                table: "Perfis");
        }
    }
}
