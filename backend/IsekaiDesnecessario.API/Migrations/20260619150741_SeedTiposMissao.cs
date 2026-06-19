using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IsekaiDesnecessario.API.Migrations
{
    /// <inheritdoc />
    public partial class SeedTiposMissao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TiposMissao",
                columns: new[] { "Id", "Nome" },
                values: new object[,]
                {
                    { 1, "Principal" },
                    { 2, "Secundária" },
                    { 3, "Desafio" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TiposMissao",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "TiposMissao",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "TiposMissao",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
