using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IsekaiDesnecessario.API.Migrations
{
    /// <inheritdoc />
    public partial class TravaDiasNoConteudo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TravaDias",
                table: "Recompensas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TravaDias",
                table: "Missoes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TravaDias",
                table: "MausHabitos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TravaDias",
                table: "BonsHabitos",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TravaDias",
                table: "Recompensas");

            migrationBuilder.DropColumn(
                name: "TravaDias",
                table: "Missoes");

            migrationBuilder.DropColumn(
                name: "TravaDias",
                table: "MausHabitos");

            migrationBuilder.DropColumn(
                name: "TravaDias",
                table: "BonsHabitos");
        }
    }
}
