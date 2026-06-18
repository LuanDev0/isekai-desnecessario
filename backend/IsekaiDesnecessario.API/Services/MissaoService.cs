using Microsoft.EntityFrameworkCore;
using IsekaiDesnecessario.API.Data;
using IsekaiDesnecessario.API.Models;

namespace IsekaiDesnecessario.API.Services;

public class MissaoService(AppDbContext db, XpService xpService)
{
    // Conclui a missão e suas secundárias vinculadas; credita XP e moedas
    // e registra tudo no diário. Retorna o perfil atualizado.
    public async Task<Perfil?> ConcluirAsync(Missao missao)
    {
        // Operação multi-tabela (missão + secundárias + XP + moedas + diário):
        // ou tudo num único commit, ou nada (SKILL.md §3 — transações explícitas).
        await using var tx = await db.Database.BeginTransactionAsync();

        missao.Concluida   = true;
        missao.ConcluidaEm = DateTime.UtcNow;
        missao.Streak++;

        // Completa missões secundárias vinculadas a esta
        var vinculadas = await db.Missoes
            .Where(m => m.MissaoPrincipalId == missao.Id && !m.Concluida)
            .ToListAsync();
        foreach (var v in vinculadas)
        {
            v.Concluida   = true;
            v.ConcluidaEm = DateTime.UtcNow;
            v.Streak++;
        }

        await db.SaveChangesAsync();

        await xpService.AdicionarXpAsync(missao.PerfilId, missao.RecompensaXp);
        foreach (var v in vinculadas)
            await xpService.AdicionarXpAsync(missao.PerfilId, v.RecompensaXp);

        var perfil = await db.Perfis.FindAsync(missao.PerfilId);
        if (perfil is not null)
        {
            perfil.Moedas += missao.RecompensaMoedas;
            foreach (var v in vinculadas)
                perfil.Moedas += v.RecompensaMoedas;
        }

        db.DiarioAcoes.Add(new DiarioAcao { PerfilId = missao.PerfilId, Emoji = "⚔️", Tipo = "missao",
            Mensagem = $"Concluiu missão \"{missao.Titulo}\" +{missao.RecompensaXp} XP +{missao.RecompensaMoedas} moedas" });
        foreach (var v in vinculadas)
            db.DiarioAcoes.Add(new DiarioAcao { PerfilId = missao.PerfilId, Emoji = "⚔️", Tipo = "missao",
                Mensagem = $"Missão vinculada \"{v.Titulo}\" concluída +{v.RecompensaXp} XP +{v.RecompensaMoedas} moedas" });

        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return perfil;
    }
}
