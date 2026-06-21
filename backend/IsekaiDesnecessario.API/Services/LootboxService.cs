using Microsoft.EntityFrameworkCore;
using IsekaiDesnecessario.API.Data;
using IsekaiDesnecessario.API.Models;

namespace IsekaiDesnecessario.API.Services;

public class LootboxService(AppDbContext db, ILogger<LootboxService> logger)
{
    // XP acumulado no dia exigido para abrir a lootbox
    public const int XpDiarioNecessario = 1000;

    public async Task<LootboxStatus?> StatusAsync(int perfilId)
    {
        var perfil = await db.Perfis.FindAsync(perfilId);
        if (perfil is null) return null;

        if (perfil.ResetarXpDiarioSeNovoDia())
            await db.SaveChangesAsync();

        bool jaAbriuHoje = perfil.UltimaLootbox?.Date == DateTime.UtcNow.Date;
        bool disponivel  = perfil.XpHoje >= XpDiarioNecessario && !jaAbriuHoje;

        return new LootboxStatus(disponivel, perfil.XpHoje, XpDiarioNecessario, jaAbriuHoje);
    }

    public async Task<LootboxResultado> AbrirAsync(int perfilId)
    {
        var perfil = await db.Perfis.FindAsync(perfilId);
        if (perfil is null) return LootboxResultado.Falha("Perfil não encontrado.");

        perfil.ResetarXpDiarioSeNovoDia();

        if (perfil.XpHoje < XpDiarioNecessario)
            return LootboxResultado.Falha($"XP insuficiente. Ganhe {XpDiarioNecessario} XP hoje para abrir a lootbox.");

        if (perfil.UltimaLootbox?.Date == DateTime.UtcNow.Date)
            return LootboxResultado.Falha("Lootbox já aberta hoje. Volte amanhã!");

        var recompensas = await db.PerfilRecompensas
            .Where(a => a.PerfilId == perfilId && a.Ativo && a.Recompensa!.Ativa)
            .Select(a => a.Recompensa!)
            .ToListAsync();

        if (recompensas.Count == 0)
            return LootboxResultado.Falha("Nenhuma recompensa cadastrada.");

        var (ganhador, chance) = SortearPonderado(recompensas);

        perfil.UltimaLootbox = DateTime.UtcNow;
        db.DiarioAcoes.Add(new DiarioAcao
        {
            PerfilId = perfilId, Emoji = "📦", Tipo = "lootbox",
            Mensagem = $"Abriu lootbox e ganhou \"{ganhador.Nome}\""
        });
        await db.SaveChangesAsync();

        logger.LogInformation("Perfil {PerfilId} abriu lootbox: ganhou \"{Recompensa}\" ({Chance}%)", perfilId, ganhador.Nome, chance);

        return LootboxResultado.Ok(ganhador, chance);
    }

    // Sorteio ponderado: peso = precoMáximo - preço + 1 (mais barato = mais chance)
    private static (Recompensa ganhador, double chance) SortearPonderado(List<Recompensa> recompensas)
    {
        int precoMax  = recompensas.Max(r => r.Preco);
        var pesos     = recompensas.Select(r => new { r, peso = precoMax - r.Preco + 1 }).ToList();
        int totalPeso = pesos.Sum(p => p.peso);
        int roll      = Random.Shared.Next(totalPeso);

        int acumulado = 0;
        var ganhador  = recompensas[0];
        foreach (var p in pesos)
        {
            acumulado += p.peso;
            if (roll < acumulado) { ganhador = p.r; break; }
        }

        int pesoGanhador = precoMax - ganhador.Preco + 1;
        double chance    = Math.Round((double)pesoGanhador / totalPeso * 100, 1);
        return (ganhador, chance);
    }
}

public record LootboxStatus(bool Disponivel, int XpHoje, int XpNecessario, bool JaAbriuHoje);

public record LootboxResultado(bool Sucesso, string? Erro, Recompensa? Recompensa, double Chance)
{
    public static LootboxResultado Ok(Recompensa recompensa, double chance) => new(true, null, recompensa, chance);
    public static LootboxResultado Falha(string erro) => new(false, erro, null, 0);
}
