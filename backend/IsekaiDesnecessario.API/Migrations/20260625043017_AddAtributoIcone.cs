using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IsekaiDesnecessario.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAtributoIcone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Icone",
                table: "Atributos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Atributos",
                keyColumn: "Id",
                keyValue: 1,
                column: "Icone",
                value: "inteligencia");

            migrationBuilder.UpdateData(
                table: "Atributos",
                keyColumn: "Id",
                keyValue: 2,
                column: "Icone",
                value: "sabedoria");

            migrationBuilder.UpdateData(
                table: "Atributos",
                keyColumn: "Id",
                keyValue: 3,
                column: "Icone",
                value: "fisico");

            migrationBuilder.UpdateData(
                table: "Atributos",
                keyColumn: "Id",
                keyValue: 4,
                column: "Icone",
                value: "disciplina");

            migrationBuilder.UpdateData(
                table: "Atributos",
                keyColumn: "Id",
                keyValue: 5,
                column: "Icone",
                value: "foco");

            migrationBuilder.UpdateData(
                table: "Atributos",
                keyColumn: "Id",
                keyValue: 6,
                column: "Icone",
                value: "vitalidade");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Icone",
                table: "Atributos");
        }
    }
}
