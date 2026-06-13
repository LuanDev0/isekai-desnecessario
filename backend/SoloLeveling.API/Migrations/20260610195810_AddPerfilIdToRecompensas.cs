using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoloLeveling.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPerfilIdToRecompensas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Descricao",
                table: "Recompensas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Emoji",
                table: "Recompensas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PerfilId",
                table: "Recompensas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Recompensas_PerfilId",
                table: "Recompensas",
                column: "PerfilId");

            migrationBuilder.AddForeignKey(
                name: "FK_Recompensas_Perfis_PerfilId",
                table: "Recompensas",
                column: "PerfilId",
                principalTable: "Perfis",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recompensas_Perfis_PerfilId",
                table: "Recompensas");

            migrationBuilder.DropIndex(
                name: "IX_Recompensas_PerfilId",
                table: "Recompensas");

            migrationBuilder.DropColumn(
                name: "Descricao",
                table: "Recompensas");

            migrationBuilder.DropColumn(
                name: "Emoji",
                table: "Recompensas");

            migrationBuilder.DropColumn(
                name: "PerfilId",
                table: "Recompensas");
        }
    }
}
