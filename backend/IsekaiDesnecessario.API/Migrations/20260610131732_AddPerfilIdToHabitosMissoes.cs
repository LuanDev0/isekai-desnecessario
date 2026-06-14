using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IsekaiDesnecessario.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPerfilIdToHabitosMissoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PerfilId",
                table: "Missoes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PerfilId",
                table: "MausHabitos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PerfilId",
                table: "BonsHabitos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Missoes_PerfilId",
                table: "Missoes",
                column: "PerfilId");

            migrationBuilder.CreateIndex(
                name: "IX_MausHabitos_PerfilId",
                table: "MausHabitos",
                column: "PerfilId");

            migrationBuilder.CreateIndex(
                name: "IX_BonsHabitos_PerfilId",
                table: "BonsHabitos",
                column: "PerfilId");

            migrationBuilder.AddForeignKey(
                name: "FK_BonsHabitos_Perfis_PerfilId",
                table: "BonsHabitos",
                column: "PerfilId",
                principalTable: "Perfis",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MausHabitos_Perfis_PerfilId",
                table: "MausHabitos",
                column: "PerfilId",
                principalTable: "Perfis",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Missoes_Perfis_PerfilId",
                table: "Missoes",
                column: "PerfilId",
                principalTable: "Perfis",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BonsHabitos_Perfis_PerfilId",
                table: "BonsHabitos");

            migrationBuilder.DropForeignKey(
                name: "FK_MausHabitos_Perfis_PerfilId",
                table: "MausHabitos");

            migrationBuilder.DropForeignKey(
                name: "FK_Missoes_Perfis_PerfilId",
                table: "Missoes");

            migrationBuilder.DropIndex(
                name: "IX_Missoes_PerfilId",
                table: "Missoes");

            migrationBuilder.DropIndex(
                name: "IX_MausHabitos_PerfilId",
                table: "MausHabitos");

            migrationBuilder.DropIndex(
                name: "IX_BonsHabitos_PerfilId",
                table: "BonsHabitos");

            migrationBuilder.DropColumn(
                name: "PerfilId",
                table: "Missoes");

            migrationBuilder.DropColumn(
                name: "PerfilId",
                table: "MausHabitos");

            migrationBuilder.DropColumn(
                name: "PerfilId",
                table: "BonsHabitos");
        }
    }
}
