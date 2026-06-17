using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IsekaiDesnecessario.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Atributos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    Emoji = table.Column<string>(type: "text", nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: false),
                    Cor = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Atributos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TiposMissao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposMissao", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GoogleId = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    EmailVerificado = table.Column<bool>(type: "boolean", nullable: false),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    FotoUrl = table.Column<string>(type: "text", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UltimoLogin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Classes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    NomeFeminino = table.Column<string>(type: "text", nullable: true),
                    Emoji = table.Column<string>(type: "text", nullable: false),
                    AtributoId = table.Column<int>(type: "integer", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "Perfis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: true),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    Genero = table.Column<string>(type: "text", nullable: true),
                    ClasseId = table.Column<int>(type: "integer", nullable: true),
                    Xp = table.Column<int>(type: "integer", nullable: false),
                    Moedas = table.Column<int>(type: "integer", nullable: false),
                    Rank = table.Column<string>(type: "text", nullable: false),
                    Titulo = table.Column<string>(type: "text", nullable: false),
                    Nivel = table.Column<int>(type: "integer", nullable: false),
                    ProximoNivelXp = table.Column<int>(type: "integer", nullable: false),
                    FotoUrl = table.Column<string>(type: "text", nullable: true),
                    XpHoje = table.Column<int>(type: "integer", nullable: false),
                    DataXpHoje = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UltimaLootbox = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DesafioRecusadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DesafioConcluídoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Perfis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Perfis_Classes_ClasseId",
                        column: x => x.ClasseId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Perfis_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "BonsHabitos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PerfilId = table.Column<int>(type: "integer", nullable: false),
                    Habito = table.Column<string>(type: "text", nullable: false),
                    Xp = table.Column<int>(type: "integer", nullable: false),
                    Frequencia = table.Column<string>(type: "text", nullable: false),
                    Streak = table.Column<int>(type: "integer", nullable: false),
                    UltimaExecucao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AtributoId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BonsHabitos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BonsHabitos_Atributos_AtributoId",
                        column: x => x.AtributoId,
                        principalTable: "Atributos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_BonsHabitos_Perfis_PerfilId",
                        column: x => x.PerfilId,
                        principalTable: "Perfis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiarioAcoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PerfilId = table.Column<int>(type: "integer", nullable: false),
                    Mensagem = table.Column<string>(type: "text", nullable: false),
                    Emoji = table.Column<string>(type: "text", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiarioAcoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiarioAcoes_Perfis_PerfilId",
                        column: x => x.PerfilId,
                        principalTable: "Perfis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Experimentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PerfilId = table.Column<int>(type: "integer", nullable: false),
                    Titulo = table.Column<string>(type: "text", nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: false),
                    DuracaoDias = table.Column<int>(type: "integer", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    Convertido = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Experimentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Experimentos_Perfis_PerfilId",
                        column: x => x.PerfilId,
                        principalTable: "Perfis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HistoricoXp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PerfilId = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<DateOnly>(type: "date", nullable: false),
                    XpHoje = table.Column<int>(type: "integer", nullable: false),
                    Nivel = table.Column<int>(type: "integer", nullable: false),
                    Moedas = table.Column<int>(type: "integer", nullable: false),
                    XpPorHoraJson = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoXp", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoricoXp_Perfis_PerfilId",
                        column: x => x.PerfilId,
                        principalTable: "Perfis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Inventario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PerfilId = table.Column<int>(type: "integer", nullable: false),
                    RecompensaId = table.Column<int>(type: "integer", nullable: false),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    Emoji = table.Column<string>(type: "text", nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: false),
                    Preco = table.Column<int>(type: "integer", nullable: false),
                    DataCompra = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataUso = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Usado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inventario_Perfis_PerfilId",
                        column: x => x.PerfilId,
                        principalTable: "Perfis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MausHabitos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PerfilId = table.Column<int>(type: "integer", nullable: false),
                    Habito = table.Column<string>(type: "text", nullable: false),
                    Xp = table.Column<int>(type: "integer", nullable: false),
                    Frequencia = table.Column<string>(type: "text", nullable: false),
                    Streak = table.Column<int>(type: "integer", nullable: false),
                    UltimaExecucao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AtributoId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MausHabitos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MausHabitos_Atributos_AtributoId",
                        column: x => x.AtributoId,
                        principalTable: "Atributos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MausHabitos_Perfis_PerfilId",
                        column: x => x.PerfilId,
                        principalTable: "Perfis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Missoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PerfilId = table.Column<int>(type: "integer", nullable: false),
                    Titulo = table.Column<string>(type: "text", nullable: false),
                    TipoId = table.Column<int>(type: "integer", nullable: false),
                    RecompensaXp = table.Column<int>(type: "integer", nullable: false),
                    RecompensaMoedas = table.Column<int>(type: "integer", nullable: false),
                    Streak = table.Column<int>(type: "integer", nullable: false),
                    Concluida = table.Column<bool>(type: "boolean", nullable: false),
                    ConcluidaEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataLimite = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MissaoPrincipalId = table.Column<int>(type: "integer", nullable: true),
                    AtributoId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Missoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Missoes_Atributos_AtributoId",
                        column: x => x.AtributoId,
                        principalTable: "Atributos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Missoes_Perfis_PerfilId",
                        column: x => x.PerfilId,
                        principalTable: "Perfis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Missoes_TiposMissao_TipoId",
                        column: x => x.TipoId,
                        principalTable: "TiposMissao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Recompensas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PerfilId = table.Column<int>(type: "integer", nullable: false),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: false),
                    Emoji = table.Column<string>(type: "text", nullable: false),
                    Preco = table.Column<int>(type: "integer", nullable: false),
                    Ativa = table.Column<bool>(type: "boolean", nullable: false),
                    AtributoId = table.Column<int>(type: "integer", nullable: true),
                    PontosNecessarios = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recompensas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recompensas_Atributos_AtributoId",
                        column: x => x.AtributoId,
                        principalTable: "Atributos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Recompensas_Perfis_PerfilId",
                        column: x => x.PerfilId,
                        principalTable: "Perfis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SnapshotsAtributo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PerfilId = table.Column<int>(type: "integer", nullable: false),
                    AtributoId = table.Column<int>(type: "integer", nullable: false),
                    Pontos = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SnapshotsAtributo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SnapshotsAtributo_Atributos_AtributoId",
                        column: x => x.AtributoId,
                        principalTable: "Atributos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SnapshotsAtributo_Perfis_PerfilId",
                        column: x => x.PerfilId,
                        principalTable: "Perfis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExperimentosDia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExperimentoId = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExperimentosDia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExperimentosDia_Experimentos_ExperimentoId",
                        column: x => x.ExperimentoId,
                        principalTable: "Experimentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Atributos",
                columns: new[] { "Id", "Cor", "Descricao", "Emoji", "Nome" },
                values: new object[,]
                {
                    { 1, "#58a6ff", "Estudar, fazer cursos, resolver exercícios", "🧠", "Inteligência" },
                    { 2, "#bc8cff", "Ler livros, podcasts, reflexão", "📚", "Sabedoria" },
                    { 3, "#3fb950", "Treinar, academia, exercícios físicos", "💪", "Físico" },
                    { 4, "#f78166", "Tarefas domésticas, rotina, pontualidade", "⚙️", "Disciplina" },
                    { 5, "#ffd700", "Pomodoro, sem celular, deep work", "🎯", "Foco" },
                    { 6, "#f85149", "Sono, hidratação, dieta, pausas", "❤️", "Vitalidade" }
                });

            migrationBuilder.InsertData(
                table: "Classes",
                columns: new[] { "Id", "AtributoId", "Emoji", "Nome", "NomeFeminino" },
                values: new object[,]
                {
                    { 1, 1, "🧙", "Mago", "Maga" },
                    { 2, 2, "🎵", "Bardo", "Barda" },
                    { 3, 3, "⚔️", "Guerreiro", "Guerreira" },
                    { 4, 4, "🛡️", "Escudeiro", "Escudeira" },
                    { 5, 5, "🎯", "Executor", "Executora" },
                    { 6, 6, "✨", "Clérigo", "Clériga" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BonsHabitos_AtributoId",
                table: "BonsHabitos",
                column: "AtributoId");

            migrationBuilder.CreateIndex(
                name: "IX_BonsHabitos_PerfilId",
                table: "BonsHabitos",
                column: "PerfilId");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_AtributoId",
                table: "Classes",
                column: "AtributoId");

            migrationBuilder.CreateIndex(
                name: "IX_DiarioAcoes_PerfilId",
                table: "DiarioAcoes",
                column: "PerfilId");

            migrationBuilder.CreateIndex(
                name: "IX_Experimentos_PerfilId",
                table: "Experimentos",
                column: "PerfilId");

            migrationBuilder.CreateIndex(
                name: "IX_ExperimentosDia_ExperimentoId",
                table: "ExperimentosDia",
                column: "ExperimentoId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricoXp_PerfilId",
                table: "HistoricoXp",
                column: "PerfilId");

            migrationBuilder.CreateIndex(
                name: "IX_Inventario_PerfilId",
                table: "Inventario",
                column: "PerfilId");

            migrationBuilder.CreateIndex(
                name: "IX_MausHabitos_AtributoId",
                table: "MausHabitos",
                column: "AtributoId");

            migrationBuilder.CreateIndex(
                name: "IX_MausHabitos_PerfilId",
                table: "MausHabitos",
                column: "PerfilId");

            migrationBuilder.CreateIndex(
                name: "IX_Missoes_AtributoId",
                table: "Missoes",
                column: "AtributoId");

            migrationBuilder.CreateIndex(
                name: "IX_Missoes_PerfilId",
                table: "Missoes",
                column: "PerfilId");

            migrationBuilder.CreateIndex(
                name: "IX_Missoes_TipoId",
                table: "Missoes",
                column: "TipoId");

            migrationBuilder.CreateIndex(
                name: "IX_Perfis_ClasseId",
                table: "Perfis",
                column: "ClasseId");

            migrationBuilder.CreateIndex(
                name: "IX_Perfis_UsuarioId",
                table: "Perfis",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Recompensas_AtributoId",
                table: "Recompensas",
                column: "AtributoId");

            migrationBuilder.CreateIndex(
                name: "IX_Recompensas_PerfilId",
                table: "Recompensas",
                column: "PerfilId");

            migrationBuilder.CreateIndex(
                name: "IX_SnapshotsAtributo_AtributoId",
                table: "SnapshotsAtributo",
                column: "AtributoId");

            migrationBuilder.CreateIndex(
                name: "IX_SnapshotsAtributo_PerfilId",
                table: "SnapshotsAtributo",
                column: "PerfilId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_GoogleId",
                table: "Usuarios",
                column: "GoogleId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BonsHabitos");

            migrationBuilder.DropTable(
                name: "DiarioAcoes");

            migrationBuilder.DropTable(
                name: "ExperimentosDia");

            migrationBuilder.DropTable(
                name: "HistoricoXp");

            migrationBuilder.DropTable(
                name: "Inventario");

            migrationBuilder.DropTable(
                name: "MausHabitos");

            migrationBuilder.DropTable(
                name: "Missoes");

            migrationBuilder.DropTable(
                name: "Recompensas");

            migrationBuilder.DropTable(
                name: "SnapshotsAtributo");

            migrationBuilder.DropTable(
                name: "Experimentos");

            migrationBuilder.DropTable(
                name: "TiposMissao");

            migrationBuilder.DropTable(
                name: "Perfis");

            migrationBuilder.DropTable(
                name: "Classes");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Atributos");
        }
    }
}
