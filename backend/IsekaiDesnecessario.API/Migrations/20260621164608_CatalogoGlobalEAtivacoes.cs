using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IsekaiDesnecessario.API.Migrations
{
    /// <inheritdoc />
    public partial class CatalogoGlobalEAtivacoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Virada pro catálogo: o conteúdo antigo era por-perfil (PerfilId) e não
            // tem como virar definição global de forma consistente. Começamos limpo
            // (decisão acordada). O Admin recria os itens do catálogo depois.
            migrationBuilder.Sql(@"DELETE FROM ""Inventario"";");
            migrationBuilder.Sql(@"DELETE FROM ""BonsHabitos"";");
            migrationBuilder.Sql(@"DELETE FROM ""MausHabitos"";");
            migrationBuilder.Sql(@"DELETE FROM ""Missoes"";");
            migrationBuilder.Sql(@"DELETE FROM ""Recompensas"";");

            migrationBuilder.DropForeignKey(
                name: "FK_BonsHabitos_Perfis_PerfilId",
                table: "BonsHabitos");

            migrationBuilder.DropForeignKey(
                name: "FK_MausHabitos_Perfis_PerfilId",
                table: "MausHabitos");

            migrationBuilder.DropForeignKey(
                name: "FK_Missoes_Perfis_PerfilId",
                table: "Missoes");

            migrationBuilder.DropForeignKey(
                name: "FK_Recompensas_Perfis_PerfilId",
                table: "Recompensas");

            migrationBuilder.DropIndex(
                name: "IX_Recompensas_PerfilId",
                table: "Recompensas");

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
                table: "Recompensas");

            migrationBuilder.DropColumn(
                name: "Concluida",
                table: "Missoes");

            migrationBuilder.DropColumn(
                name: "ConcluidaEm",
                table: "Missoes");

            migrationBuilder.DropColumn(
                name: "PerfilId",
                table: "Missoes");

            migrationBuilder.DropColumn(
                name: "Streak",
                table: "Missoes");

            migrationBuilder.DropColumn(
                name: "PerfilId",
                table: "MausHabitos");

            migrationBuilder.DropColumn(
                name: "Streak",
                table: "MausHabitos");

            migrationBuilder.DropColumn(
                name: "UltimaExecucao",
                table: "MausHabitos");

            migrationBuilder.DropColumn(
                name: "PerfilId",
                table: "BonsHabitos");

            migrationBuilder.DropColumn(
                name: "Streak",
                table: "BonsHabitos");

            migrationBuilder.DropColumn(
                name: "UltimaExecucao",
                table: "BonsHabitos");

            migrationBuilder.AddColumn<int>(
                name: "CriadoPorUsuarioId",
                table: "Recompensas",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Escopo",
                table: "Recompensas",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Global");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Recompensas",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Aprovado");

            migrationBuilder.AddColumn<int>(
                name: "CriadoPorUsuarioId",
                table: "Missoes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Escopo",
                table: "Missoes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Global");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Missoes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Aprovado");

            migrationBuilder.AddColumn<int>(
                name: "CriadoPorUsuarioId",
                table: "MausHabitos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Escopo",
                table: "MausHabitos",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Global");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "MausHabitos",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Aprovado");

            migrationBuilder.AddColumn<int>(
                name: "CriadoPorUsuarioId",
                table: "BonsHabitos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Escopo",
                table: "BonsHabitos",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Global");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "BonsHabitos",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Aprovado");

            migrationBuilder.CreateTable(
                name: "PerfilBonsHabitos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PerfilId = table.Column<int>(type: "integer", nullable: false),
                    BomHabitoId = table.Column<int>(type: "integer", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    Streak = table.Column<int>(type: "integer", nullable: false),
                    UltimaExecucao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TravadoAte = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfilBonsHabitos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerfilBonsHabitos_BonsHabitos_BomHabitoId",
                        column: x => x.BomHabitoId,
                        principalTable: "BonsHabitos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PerfilBonsHabitos_Perfis_PerfilId",
                        column: x => x.PerfilId,
                        principalTable: "Perfis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PerfilMausHabitos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PerfilId = table.Column<int>(type: "integer", nullable: false),
                    MauHabitoId = table.Column<int>(type: "integer", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    Streak = table.Column<int>(type: "integer", nullable: false),
                    UltimaExecucao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TravadoAte = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfilMausHabitos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerfilMausHabitos_MausHabitos_MauHabitoId",
                        column: x => x.MauHabitoId,
                        principalTable: "MausHabitos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PerfilMausHabitos_Perfis_PerfilId",
                        column: x => x.PerfilId,
                        principalTable: "Perfis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PerfilMissoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PerfilId = table.Column<int>(type: "integer", nullable: false),
                    MissaoId = table.Column<int>(type: "integer", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    Concluida = table.Column<bool>(type: "boolean", nullable: false),
                    ConcluidaEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Streak = table.Column<int>(type: "integer", nullable: false),
                    TravadoAte = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfilMissoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerfilMissoes_Missoes_MissaoId",
                        column: x => x.MissaoId,
                        principalTable: "Missoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PerfilMissoes_Perfis_PerfilId",
                        column: x => x.PerfilId,
                        principalTable: "Perfis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PerfilRecompensas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PerfilId = table.Column<int>(type: "integer", nullable: false),
                    RecompensaId = table.Column<int>(type: "integer", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    TravadoAte = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfilRecompensas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerfilRecompensas_Perfis_PerfilId",
                        column: x => x.PerfilId,
                        principalTable: "Perfis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PerfilRecompensas_Recompensas_RecompensaId",
                        column: x => x.RecompensaId,
                        principalTable: "Recompensas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Recompensas_CriadoPorUsuarioId",
                table: "Recompensas",
                column: "CriadoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Missoes_CriadoPorUsuarioId",
                table: "Missoes",
                column: "CriadoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_MausHabitos_CriadoPorUsuarioId",
                table: "MausHabitos",
                column: "CriadoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_BonsHabitos_CriadoPorUsuarioId",
                table: "BonsHabitos",
                column: "CriadoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_PerfilBonsHabitos_BomHabitoId",
                table: "PerfilBonsHabitos",
                column: "BomHabitoId");

            migrationBuilder.CreateIndex(
                name: "IX_PerfilBonsHabitos_PerfilId_BomHabitoId",
                table: "PerfilBonsHabitos",
                columns: new[] { "PerfilId", "BomHabitoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PerfilMausHabitos_MauHabitoId",
                table: "PerfilMausHabitos",
                column: "MauHabitoId");

            migrationBuilder.CreateIndex(
                name: "IX_PerfilMausHabitos_PerfilId_MauHabitoId",
                table: "PerfilMausHabitos",
                columns: new[] { "PerfilId", "MauHabitoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PerfilMissoes_MissaoId",
                table: "PerfilMissoes",
                column: "MissaoId");

            migrationBuilder.CreateIndex(
                name: "IX_PerfilMissoes_PerfilId_MissaoId",
                table: "PerfilMissoes",
                columns: new[] { "PerfilId", "MissaoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PerfilRecompensas_PerfilId_RecompensaId",
                table: "PerfilRecompensas",
                columns: new[] { "PerfilId", "RecompensaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PerfilRecompensas_RecompensaId",
                table: "PerfilRecompensas",
                column: "RecompensaId");

            migrationBuilder.AddForeignKey(
                name: "FK_BonsHabitos_Usuarios_CriadoPorUsuarioId",
                table: "BonsHabitos",
                column: "CriadoPorUsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MausHabitos_Usuarios_CriadoPorUsuarioId",
                table: "MausHabitos",
                column: "CriadoPorUsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Missoes_Usuarios_CriadoPorUsuarioId",
                table: "Missoes",
                column: "CriadoPorUsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Recompensas_Usuarios_CriadoPorUsuarioId",
                table: "Recompensas",
                column: "CriadoPorUsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BonsHabitos_Usuarios_CriadoPorUsuarioId",
                table: "BonsHabitos");

            migrationBuilder.DropForeignKey(
                name: "FK_MausHabitos_Usuarios_CriadoPorUsuarioId",
                table: "MausHabitos");

            migrationBuilder.DropForeignKey(
                name: "FK_Missoes_Usuarios_CriadoPorUsuarioId",
                table: "Missoes");

            migrationBuilder.DropForeignKey(
                name: "FK_Recompensas_Usuarios_CriadoPorUsuarioId",
                table: "Recompensas");

            migrationBuilder.DropTable(
                name: "PerfilBonsHabitos");

            migrationBuilder.DropTable(
                name: "PerfilMausHabitos");

            migrationBuilder.DropTable(
                name: "PerfilMissoes");

            migrationBuilder.DropTable(
                name: "PerfilRecompensas");

            migrationBuilder.DropIndex(
                name: "IX_Recompensas_CriadoPorUsuarioId",
                table: "Recompensas");

            migrationBuilder.DropIndex(
                name: "IX_Missoes_CriadoPorUsuarioId",
                table: "Missoes");

            migrationBuilder.DropIndex(
                name: "IX_MausHabitos_CriadoPorUsuarioId",
                table: "MausHabitos");

            migrationBuilder.DropIndex(
                name: "IX_BonsHabitos_CriadoPorUsuarioId",
                table: "BonsHabitos");

            migrationBuilder.DropColumn(
                name: "CriadoPorUsuarioId",
                table: "Recompensas");

            migrationBuilder.DropColumn(
                name: "Escopo",
                table: "Recompensas");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Recompensas");

            migrationBuilder.DropColumn(
                name: "CriadoPorUsuarioId",
                table: "Missoes");

            migrationBuilder.DropColumn(
                name: "Escopo",
                table: "Missoes");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Missoes");

            migrationBuilder.DropColumn(
                name: "CriadoPorUsuarioId",
                table: "MausHabitos");

            migrationBuilder.DropColumn(
                name: "Escopo",
                table: "MausHabitos");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "MausHabitos");

            migrationBuilder.DropColumn(
                name: "CriadoPorUsuarioId",
                table: "BonsHabitos");

            migrationBuilder.DropColumn(
                name: "Escopo",
                table: "BonsHabitos");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "BonsHabitos");

            migrationBuilder.AddColumn<int>(
                name: "PerfilId",
                table: "Recompensas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Concluida",
                table: "Missoes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConcluidaEm",
                table: "Missoes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PerfilId",
                table: "Missoes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Streak",
                table: "Missoes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PerfilId",
                table: "MausHabitos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Streak",
                table: "MausHabitos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimaExecucao",
                table: "MausHabitos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PerfilId",
                table: "BonsHabitos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Streak",
                table: "BonsHabitos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimaExecucao",
                table: "BonsHabitos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recompensas_PerfilId",
                table: "Recompensas",
                column: "PerfilId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Recompensas_Perfis_PerfilId",
                table: "Recompensas",
                column: "PerfilId",
                principalTable: "Perfis",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
