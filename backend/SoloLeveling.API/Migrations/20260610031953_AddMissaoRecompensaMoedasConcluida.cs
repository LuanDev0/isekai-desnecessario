using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoloLeveling.API.Migrations
{
    /// <inheritdoc />
    public partial class AddMissaoRecompensaMoedasConcluida : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Concluida",
                table: "Missoes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RecompensaMoedas",
                table: "Missoes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Concluida",
                table: "Missoes");

            migrationBuilder.DropColumn(
                name: "RecompensaMoedas",
                table: "Missoes");
        }
    }
}
