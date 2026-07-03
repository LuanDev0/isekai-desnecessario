using Microsoft.EntityFrameworkCore;
using IsekaiDesnecessario.API.Models;

namespace IsekaiDesnecessario.API.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Perfil> Perfis => Set<Perfil>();
    public DbSet<Classe> Classes => Set<Classe>();
    // ── Definições de conteúdo (catálogo global / próprio) ──
    public DbSet<BomHabito> BonsHabitos => Set<BomHabito>();
    public DbSet<MauHabito> MausHabitos => Set<MauHabito>();
    public DbSet<Missao> Missoes => Set<Missao>();
    public DbSet<TipoMissao> TiposMissao => Set<TipoMissao>();
    public DbSet<Recompensa> Recompensas => Set<Recompensa>();
    // ── Ativações por perfil (estado por-perfil de cada definição) ──
    public DbSet<PerfilBomHabito> PerfilBonsHabitos => Set<PerfilBomHabito>();
    public DbSet<PerfilMauHabito> PerfilMausHabitos => Set<PerfilMauHabito>();
    public DbSet<PerfilMissao> PerfilMissoes => Set<PerfilMissao>();
    public DbSet<PerfilRecompensa> PerfilRecompensas => Set<PerfilRecompensa>();
    public DbSet<HistoricoXp> HistoricoXp => Set<HistoricoXp>();
    public DbSet<ItemInventario> Inventario => Set<ItemInventario>();
    public DbSet<Atributo>         Atributos        => Set<Atributo>();
    public DbSet<DiarioAcao>       DiarioAcoes      => Set<DiarioAcao>();
    public DbSet<SnapshotAtributo> SnapshotsAtributo => Set<SnapshotAtributo>();
    public DbSet<Experimento>    Experimentos    => Set<Experimento>();
    public DbSet<ExperimentoDia> ExperimentosDia => Set<ExperimentoDia>();
    public DbSet<Notificacao>    Notificacoes    => Set<Notificacao>();
    public DbSet<PontosAtributo> PontosAtributos => Set<PontosAtributo>();
    // ── Grupos (assinatura separada do VIP; economia própria) ──
    public DbSet<Grupo>                  Grupos                  => Set<Grupo>();
    public DbSet<GrupoMembro>            GrupoMembros            => Set<GrupoMembro>();
    public DbSet<GrupoConvite>           GrupoConvites           => Set<GrupoConvite>();
    public DbSet<GrupoHabito>            GrupoHabitos            => Set<GrupoHabito>();
    public DbSet<GrupoMissao>            GrupoMissoes            => Set<GrupoMissao>();
    public DbSet<GrupoRecompensa>        GrupoRecompensas        => Set<GrupoRecompensa>();
    public DbSet<GrupoHabitoExecucao>    GrupoHabitoExecucoes    => Set<GrupoHabitoExecucao>();
    public DbSet<GrupoMissaoConclusao>   GrupoMissaoConclusoes   => Set<GrupoMissaoConclusao>();
    public DbSet<GrupoRecompensaResgate> GrupoRecompensaResgates => Set<GrupoRecompensaResgate>();
    public DbSet<GrupoFeedEvento>        GrupoFeedEventos        => Set<GrupoFeedEvento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── Conta Google (Usuario) ────────────────────────
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.GoogleId)
            .IsUnique();
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Papel de acesso gravado como texto (ex.: "Admin") — facilita ajuste manual.
        modelBuilder.Entity<Usuario>()
            .Property(u => u.Role)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(Role.Usuario);

        // Notificações: apagar a conta apaga suas notificações
        modelBuilder.Entity<Notificacao>()
            .HasOne(n => n.Usuario)
            .WithMany()
            .HasForeignKey(n => n.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Notificacao>()
            .HasIndex(n => new { n.UsuarioId, n.Lida });

        // Um usuário tem vários perfis; apagar a conta NÃO apaga os heróis
        // (eles viram convidados — UsuarioId = null), preservando o progresso.
        modelBuilder.Entity<Perfil>()
            .HasOne(p => p.Usuario)
            .WithMany(u => u.Perfis)
            .HasForeignKey(p => p.UsuarioId)
            .OnDelete(DeleteBehavior.SetNull);

        // ── Classe ────────────────────────────────────────
        modelBuilder.Entity<Perfil>()
            .HasOne(p => p.Classe)
            .WithMany(c => c.Perfis)
            .HasForeignKey(p => p.ClasseId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Classe>().HasData(
            new Classe { Id = 1, Nome = "Mago",      NomeFeminino = "Maga",      Emoji = "🧙", AtributoId = 1 },
            new Classe { Id = 2, Nome = "Bardo",     NomeFeminino = "Barda",     Emoji = "🎵", AtributoId = 2 },
            new Classe { Id = 3, Nome = "Guerreiro", NomeFeminino = "Guerreira", Emoji = "⚔️", AtributoId = 3 },
            new Classe { Id = 4, Nome = "Escudeiro", NomeFeminino = "Escudeira", Emoji = "🛡️", AtributoId = 4 },
            new Classe { Id = 5, Nome = "Executor",  NomeFeminino = "Executora", Emoji = "🎯", AtributoId = 5 },
            new Classe { Id = 6, Nome = "Clérigo",   NomeFeminino = "Clériga",   Emoji = "✨", AtributoId = 6 }
        );

        modelBuilder.Entity<TipoMissao>().HasData(
            new TipoMissao { Id = 1, Nome = "Principal"  },
            new TipoMissao { Id = 2, Nome = "Secundária" },
            new TipoMissao { Id = 3, Nome = "Desafio"    }
        );

        modelBuilder.Entity<Missao>()
            .HasOne(m => m.Tipo)
            .WithMany(t => t.Missoes)
            .HasForeignKey(m => m.TipoId);

        modelBuilder.Entity<Missao>()
            .HasOne(m => m.Atributo)
            .WithMany(a => a.Missoes)
            .HasForeignKey(m => m.AtributoId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<BomHabito>()
            .HasOne(h => h.Atributo)
            .WithMany(a => a.BonsHabitos)
            .HasForeignKey(h => h.AtributoId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<MauHabito>()
            .HasOne(h => h.Atributo)
            .WithMany(a => a.MausHabitos)
            .HasForeignKey(h => h.AtributoId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Recompensa>()
            .HasOne(r => r.Atributo)
            .WithMany()
            .HasForeignKey(r => r.AtributoId)
            .OnDelete(DeleteBehavior.SetNull);

        // ── Catálogo: escopo/status (texto) + autor (SetNull ao apagar a conta) ──
        foreach (var tipo in new[] { typeof(BomHabito), typeof(MauHabito), typeof(Missao), typeof(Recompensa) })
        {
            modelBuilder.Entity(tipo).Property("Escopo")
                .HasConversion<string>().HasMaxLength(20).HasDefaultValue(EscopoConteudo.Global);
            modelBuilder.Entity(tipo).Property("Status")
                .HasConversion<string>().HasMaxLength(20).HasDefaultValue(StatusConteudo.Aprovado);
        }

        modelBuilder.Entity<BomHabito>()
            .HasOne(d => d.CriadoPor).WithMany().HasForeignKey(d => d.CriadoPorUsuarioId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<MauHabito>()
            .HasOne(d => d.CriadoPor).WithMany().HasForeignKey(d => d.CriadoPorUsuarioId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<Missao>()
            .HasOne(d => d.CriadoPor).WithMany().HasForeignKey(d => d.CriadoPorUsuarioId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<Recompensa>()
            .HasOne(d => d.CriadoPor).WithMany().HasForeignKey(d => d.CriadoPorUsuarioId).OnDelete(DeleteBehavior.SetNull);

        // ── Vínculo multi-classe (N:N item ↔ Classe); join tables implícitas ──
        modelBuilder.Entity<BomHabito>().HasMany(d => d.Classes).WithMany();
        modelBuilder.Entity<MauHabito>().HasMany(d => d.Classes).WithMany();
        modelBuilder.Entity<Missao>().HasMany(d => d.Classes).WithMany();
        modelBuilder.Entity<Recompensa>().HasMany(d => d.Classes).WithMany();

        // ── Ativações (Perfil ↔ definição); apagar perfil OU definição apaga a ativação ──
        modelBuilder.Entity<PerfilBomHabito>(e =>
        {
            e.HasOne(a => a.Perfil).WithMany(p => p.BonsHabitos)
                .HasForeignKey(a => a.PerfilId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.BomHabito).WithMany(d => d.Ativacoes)
                .HasForeignKey(a => a.BomHabitoId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(a => new { a.PerfilId, a.BomHabitoId }).IsUnique();
        });
        modelBuilder.Entity<PerfilMauHabito>(e =>
        {
            e.HasOne(a => a.Perfil).WithMany(p => p.MausHabitos)
                .HasForeignKey(a => a.PerfilId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.MauHabito).WithMany(d => d.Ativacoes)
                .HasForeignKey(a => a.MauHabitoId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(a => new { a.PerfilId, a.MauHabitoId }).IsUnique();
        });
        modelBuilder.Entity<PerfilMissao>(e =>
        {
            e.HasOne(a => a.Perfil).WithMany(p => p.Missoes)
                .HasForeignKey(a => a.PerfilId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.Missao).WithMany(d => d.Ativacoes)
                .HasForeignKey(a => a.MissaoId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(a => new { a.PerfilId, a.MissaoId }).IsUnique();
        });
        modelBuilder.Entity<PerfilRecompensa>(e =>
        {
            e.HasOne(a => a.Perfil).WithMany(p => p.Recompensas)
                .HasForeignKey(a => a.PerfilId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.Recompensa).WithMany(d => d.Ativacoes)
                .HasForeignKey(a => a.RecompensaId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(a => new { a.PerfilId, a.RecompensaId }).IsUnique();
        });

        modelBuilder.Entity<PontosAtributo>(e =>
        {
            e.HasKey(p => new { p.PerfilId, p.AtributoId });
            e.HasOne(p => p.Perfil).WithMany().HasForeignKey(p => p.PerfilId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(p => p.Atributo).WithMany().HasForeignKey(p => p.AtributoId).OnDelete(DeleteBehavior.Cascade);
        });

        // ── Grupos ────────────────────────────────────────
        // Apagar a conta do Organizador apaga o grupo inteiro (membros perdem acesso).
        modelBuilder.Entity<Grupo>()
            .HasOne(g => g.Organizador).WithMany()
            .HasForeignKey(g => g.OrganizadorUsuarioId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Grupo>()
            .Property(g => g.Plano).HasMaxLength(20);

        // Um perfil participa de um grupo uma única vez; apagar perfil ou grupo apaga a participação.
        modelBuilder.Entity<GrupoMembro>(e =>
        {
            e.HasOne(m => m.Grupo).WithMany(g => g.Membros)
                .HasForeignKey(m => m.GrupoId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(m => m.Perfil).WithMany()
                .HasForeignKey(m => m.PerfilId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(m => new { m.GrupoId, m.PerfilId }).IsUnique();
        });

        modelBuilder.Entity<GrupoConvite>(e =>
        {
            e.HasOne(c => c.Grupo).WithMany(g => g.Convites)
                .HasForeignKey(c => c.GrupoId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(c => c.Usuario).WithMany()
                .HasForeignKey(c => c.UsuarioId).OnDelete(DeleteBehavior.Cascade);
            e.Property(c => c.Status).HasMaxLength(20);
            e.HasIndex(c => new { c.UsuarioId, c.Status });
        });

        modelBuilder.Entity<GrupoHabito>()
            .HasOne(h => h.Grupo).WithMany(g => g.Habitos)
            .HasForeignKey(h => h.GrupoId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<GrupoMissao>()
            .HasOne(m => m.Grupo).WithMany(g => g.Missoes)
            .HasForeignKey(m => m.GrupoId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<GrupoRecompensa>()
            .HasOne(r => r.Grupo).WithMany(g => g.Recompensas)
            .HasForeignKey(r => r.GrupoId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GrupoHabitoExecucao>(e =>
        {
            e.HasOne(x => x.GrupoHabito).WithMany(h => h.Execucoes)
                .HasForeignKey(x => x.GrupoHabitoId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.GrupoMembro).WithMany()
                .HasForeignKey(x => x.GrupoMembroId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.GrupoHabitoId, x.GrupoMembroId });
        });

        // Cada membro conclui uma missão do grupo uma única vez.
        modelBuilder.Entity<GrupoMissaoConclusao>(e =>
        {
            e.HasOne(x => x.GrupoMissao).WithMany(m => m.Conclusoes)
                .HasForeignKey(x => x.GrupoMissaoId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.GrupoMembro).WithMany()
                .HasForeignKey(x => x.GrupoMembroId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.GrupoMissaoId, x.GrupoMembroId }).IsUnique();
        });

        modelBuilder.Entity<GrupoRecompensaResgate>(e =>
        {
            e.HasOne(x => x.GrupoRecompensa).WithMany(r => r.Resgates)
                .HasForeignKey(x => x.GrupoRecompensaId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.GrupoMembro).WithMany()
                .HasForeignKey(x => x.GrupoMembroId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GrupoFeedEvento>(e =>
        {
            e.HasOne(f => f.Grupo).WithMany(g => g.Feed)
                .HasForeignKey(f => f.GrupoId).OnDelete(DeleteBehavior.Cascade);
            e.Property(f => f.Tipo).HasMaxLength(20);
            e.HasIndex(f => new { f.GrupoId, f.CriadoEm });
        });

        modelBuilder.Entity<Atributo>().HasData(
            new Atributo { Id = 1, Nome = "Inteligência", Emoji = "🧠", Icone = "inteligencia", Descricao = "Estudar, fazer cursos, resolver exercícios", Cor = "#58a6ff" },
            new Atributo { Id = 2, Nome = "Sabedoria",    Emoji = "📚", Icone = "sabedoria",    Descricao = "Ler livros, podcasts, reflexão",             Cor = "#bc8cff" },
            new Atributo { Id = 3, Nome = "Físico",       Emoji = "💪", Icone = "fisico",       Descricao = "Treinar, academia, exercícios físicos",      Cor = "#3fb950" },
            new Atributo { Id = 4, Nome = "Disciplina",   Emoji = "⚙️", Icone = "disciplina",   Descricao = "Tarefas domésticas, rotina, pontualidade",  Cor = "#f78166" },
            new Atributo { Id = 5, Nome = "Foco",         Emoji = "🎯", Icone = "foco",         Descricao = "Pomodoro, sem celular, deep work",          Cor = "#ffd700" },
            new Atributo { Id = 6, Nome = "Vitalidade",   Emoji = "❤️", Icone = "vitalidade",   Descricao = "Sono, hidratação, dieta, pausas",          Cor = "#f85149" }
        );
    }
}
