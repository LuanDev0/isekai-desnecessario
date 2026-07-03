using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IsekaiDesnecessario.API.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarGrupos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Grupos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    OrganizadorUsuarioId = table.Column<int>(type: "integer", nullable: false),
                    Plano = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MaxMembros = table.Column<int>(type: "integer", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grupos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Grupos_Usuarios_OrganizadorUsuarioId",
                        column: x => x.OrganizadorUsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GrupoConvites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GrupoId = table.Column<int>(type: "integer", nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrupoConvites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrupoConvites_Grupos_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "Grupos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GrupoConvites_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GrupoFeedEventos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GrupoId = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Mensagem = table.Column<string>(type: "text", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrupoFeedEventos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrupoFeedEventos_Grupos_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "Grupos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GrupoHabitos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GrupoId = table.Column<int>(type: "integer", nullable: false),
                    Habito = table.Column<string>(type: "text", nullable: false),
                    Xp = table.Column<int>(type: "integer", nullable: false),
                    Frequencia = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrupoHabitos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrupoHabitos_Grupos_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "Grupos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GrupoMembros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GrupoId = table.Column<int>(type: "integer", nullable: false),
                    PerfilId = table.Column<int>(type: "integer", nullable: false),
                    XpGrupo = table.Column<int>(type: "integer", nullable: false),
                    MoedasGrupo = table.Column<int>(type: "integer", nullable: false),
                    EntrouEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrupoMembros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrupoMembros_Grupos_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "Grupos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GrupoMembros_Perfis_PerfilId",
                        column: x => x.PerfilId,
                        principalTable: "Perfis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GrupoMissoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GrupoId = table.Column<int>(type: "integer", nullable: false),
                    Titulo = table.Column<string>(type: "text", nullable: false),
                    RecompensaXp = table.Column<int>(type: "integer", nullable: false),
                    RecompensaMoedas = table.Column<int>(type: "integer", nullable: false),
                    DataLimite = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrupoMissoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrupoMissoes_Grupos_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "Grupos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GrupoRecompensas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GrupoId = table.Column<int>(type: "integer", nullable: false),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    Custo = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrupoRecompensas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrupoRecompensas_Grupos_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "Grupos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GrupoHabitoExecucoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GrupoHabitoId = table.Column<int>(type: "integer", nullable: false),
                    GrupoMembroId = table.Column<int>(type: "integer", nullable: false),
                    ExecutadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrupoHabitoExecucoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrupoHabitoExecucoes_GrupoHabitos_GrupoHabitoId",
                        column: x => x.GrupoHabitoId,
                        principalTable: "GrupoHabitos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GrupoHabitoExecucoes_GrupoMembros_GrupoMembroId",
                        column: x => x.GrupoMembroId,
                        principalTable: "GrupoMembros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GrupoMissaoConclusoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GrupoMissaoId = table.Column<int>(type: "integer", nullable: false),
                    GrupoMembroId = table.Column<int>(type: "integer", nullable: false),
                    ConcluidaEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrupoMissaoConclusoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrupoMissaoConclusoes_GrupoMembros_GrupoMembroId",
                        column: x => x.GrupoMembroId,
                        principalTable: "GrupoMembros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GrupoMissaoConclusoes_GrupoMissoes_GrupoMissaoId",
                        column: x => x.GrupoMissaoId,
                        principalTable: "GrupoMissoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GrupoRecompensaResgates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GrupoRecompensaId = table.Column<int>(type: "integer", nullable: false),
                    GrupoMembroId = table.Column<int>(type: "integer", nullable: false),
                    ResgatadaEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrupoRecompensaResgates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrupoRecompensaResgates_GrupoMembros_GrupoMembroId",
                        column: x => x.GrupoMembroId,
                        principalTable: "GrupoMembros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GrupoRecompensaResgates_GrupoRecompensas_GrupoRecompensaId",
                        column: x => x.GrupoRecompensaId,
                        principalTable: "GrupoRecompensas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GrupoConvites_GrupoId",
                table: "GrupoConvites",
                column: "GrupoId");

            migrationBuilder.CreateIndex(
                name: "IX_GrupoConvites_UsuarioId_Status",
                table: "GrupoConvites",
                columns: new[] { "UsuarioId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_GrupoFeedEventos_GrupoId_CriadoEm",
                table: "GrupoFeedEventos",
                columns: new[] { "GrupoId", "CriadoEm" });

            migrationBuilder.CreateIndex(
                name: "IX_GrupoHabitoExecucoes_GrupoHabitoId_GrupoMembroId",
                table: "GrupoHabitoExecucoes",
                columns: new[] { "GrupoHabitoId", "GrupoMembroId" });

            migrationBuilder.CreateIndex(
                name: "IX_GrupoHabitoExecucoes_GrupoMembroId",
                table: "GrupoHabitoExecucoes",
                column: "GrupoMembroId");

            migrationBuilder.CreateIndex(
                name: "IX_GrupoHabitos_GrupoId",
                table: "GrupoHabitos",
                column: "GrupoId");

            migrationBuilder.CreateIndex(
                name: "IX_GrupoMembros_GrupoId_PerfilId",
                table: "GrupoMembros",
                columns: new[] { "GrupoId", "PerfilId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GrupoMembros_PerfilId",
                table: "GrupoMembros",
                column: "PerfilId");

            migrationBuilder.CreateIndex(
                name: "IX_GrupoMissaoConclusoes_GrupoMembroId",
                table: "GrupoMissaoConclusoes",
                column: "GrupoMembroId");

            migrationBuilder.CreateIndex(
                name: "IX_GrupoMissaoConclusoes_GrupoMissaoId_GrupoMembroId",
                table: "GrupoMissaoConclusoes",
                columns: new[] { "GrupoMissaoId", "GrupoMembroId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GrupoMissoes_GrupoId",
                table: "GrupoMissoes",
                column: "GrupoId");

            migrationBuilder.CreateIndex(
                name: "IX_GrupoRecompensaResgates_GrupoMembroId",
                table: "GrupoRecompensaResgates",
                column: "GrupoMembroId");

            migrationBuilder.CreateIndex(
                name: "IX_GrupoRecompensaResgates_GrupoRecompensaId",
                table: "GrupoRecompensaResgates",
                column: "GrupoRecompensaId");

            migrationBuilder.CreateIndex(
                name: "IX_GrupoRecompensas_GrupoId",
                table: "GrupoRecompensas",
                column: "GrupoId");

            migrationBuilder.CreateIndex(
                name: "IX_Grupos_OrganizadorUsuarioId",
                table: "Grupos",
                column: "OrganizadorUsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GrupoConvites");

            migrationBuilder.DropTable(
                name: "GrupoFeedEventos");

            migrationBuilder.DropTable(
                name: "GrupoHabitoExecucoes");

            migrationBuilder.DropTable(
                name: "GrupoMissaoConclusoes");

            migrationBuilder.DropTable(
                name: "GrupoRecompensaResgates");

            migrationBuilder.DropTable(
                name: "GrupoHabitos");

            migrationBuilder.DropTable(
                name: "GrupoMissoes");

            migrationBuilder.DropTable(
                name: "GrupoMembros");

            migrationBuilder.DropTable(
                name: "GrupoRecompensas");

            migrationBuilder.DropTable(
                name: "Grupos");
        }
    }
}
