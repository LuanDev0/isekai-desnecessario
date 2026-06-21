using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IsekaiDesnecessario.API.Migrations
{
    /// <inheritdoc />
    public partial class VinculoMultiClasse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BomHabitoClasse",
                columns: table => new
                {
                    BomHabitoId = table.Column<int>(type: "integer", nullable: false),
                    ClassesId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BomHabitoClasse", x => new { x.BomHabitoId, x.ClassesId });
                    table.ForeignKey(
                        name: "FK_BomHabitoClasse_BonsHabitos_BomHabitoId",
                        column: x => x.BomHabitoId,
                        principalTable: "BonsHabitos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BomHabitoClasse_Classes_ClassesId",
                        column: x => x.ClassesId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClasseMauHabito",
                columns: table => new
                {
                    ClassesId = table.Column<int>(type: "integer", nullable: false),
                    MauHabitoId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClasseMauHabito", x => new { x.ClassesId, x.MauHabitoId });
                    table.ForeignKey(
                        name: "FK_ClasseMauHabito_Classes_ClassesId",
                        column: x => x.ClassesId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClasseMauHabito_MausHabitos_MauHabitoId",
                        column: x => x.MauHabitoId,
                        principalTable: "MausHabitos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClasseMissao",
                columns: table => new
                {
                    ClassesId = table.Column<int>(type: "integer", nullable: false),
                    MissaoId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClasseMissao", x => new { x.ClassesId, x.MissaoId });
                    table.ForeignKey(
                        name: "FK_ClasseMissao_Classes_ClassesId",
                        column: x => x.ClassesId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClasseMissao_Missoes_MissaoId",
                        column: x => x.MissaoId,
                        principalTable: "Missoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClasseRecompensa",
                columns: table => new
                {
                    ClassesId = table.Column<int>(type: "integer", nullable: false),
                    RecompensaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClasseRecompensa", x => new { x.ClassesId, x.RecompensaId });
                    table.ForeignKey(
                        name: "FK_ClasseRecompensa_Classes_ClassesId",
                        column: x => x.ClassesId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClasseRecompensa_Recompensas_RecompensaId",
                        column: x => x.RecompensaId,
                        principalTable: "Recompensas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BomHabitoClasse_ClassesId",
                table: "BomHabitoClasse",
                column: "ClassesId");

            migrationBuilder.CreateIndex(
                name: "IX_ClasseMauHabito_MauHabitoId",
                table: "ClasseMauHabito",
                column: "MauHabitoId");

            migrationBuilder.CreateIndex(
                name: "IX_ClasseMissao_MissaoId",
                table: "ClasseMissao",
                column: "MissaoId");

            migrationBuilder.CreateIndex(
                name: "IX_ClasseRecompensa_RecompensaId",
                table: "ClasseRecompensa",
                column: "RecompensaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BomHabitoClasse");

            migrationBuilder.DropTable(
                name: "ClasseMauHabito");

            migrationBuilder.DropTable(
                name: "ClasseMissao");

            migrationBuilder.DropTable(
                name: "ClasseRecompensa");
        }
    }
}
