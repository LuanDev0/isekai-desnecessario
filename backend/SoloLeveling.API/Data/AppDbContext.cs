using Microsoft.EntityFrameworkCore;
using SoloLeveling.API.Models;

namespace SoloLeveling.API.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Perfil> Perfis => Set<Perfil>();
    public DbSet<BomHabito> BonsHabitos => Set<BomHabito>();
    public DbSet<MauHabito> MausHabitos => Set<MauHabito>();
    public DbSet<Missao> Missoes => Set<Missao>();
    public DbSet<TipoMissao> TiposMissao => Set<TipoMissao>();
    public DbSet<Recompensa> Recompensas => Set<Recompensa>();
    public DbSet<HistoricoXp> HistoricoXp => Set<HistoricoXp>();
    public DbSet<ItemInventario> Inventario => Set<ItemInventario>();
    public DbSet<Atributo>         Atributos        => Set<Atributo>();
    public DbSet<DiarioAcao>       DiarioAcoes      => Set<DiarioAcao>();
    public DbSet<SnapshotAtributo> SnapshotsAtributo => Set<SnapshotAtributo>();
    public DbSet<Experimento>    Experimentos    => Set<Experimento>();
    public DbSet<ExperimentoDia> ExperimentosDia => Set<ExperimentoDia>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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

        modelBuilder.Entity<Atributo>().HasData(
            new Atributo { Id = 1, Nome = "Inteligência", Emoji = "🧠", Descricao = "Estudar, fazer cursos, resolver exercícios", Cor = "#58a6ff" },
            new Atributo { Id = 2, Nome = "Sabedoria",    Emoji = "📚", Descricao = "Ler livros, podcasts, reflexão",             Cor = "#bc8cff" },
            new Atributo { Id = 3, Nome = "Físico",       Emoji = "💪", Descricao = "Treinar, academia, exercícios físicos",      Cor = "#3fb950" },
            new Atributo { Id = 4, Nome = "Disciplina",   Emoji = "⚙️", Descricao = "Tarefas domésticas, rotina, pontualidade",  Cor = "#f78166" },
            new Atributo { Id = 5, Nome = "Foco",         Emoji = "🎯", Descricao = "Pomodoro, sem celular, deep work",          Cor = "#ffd700" },
            new Atributo { Id = 6, Nome = "Vitalidade",   Emoji = "❤️", Descricao = "Sono, hidratação, dieta, pausas",          Cor = "#f85149" }
        );
    }
}
