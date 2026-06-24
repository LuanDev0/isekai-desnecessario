using Microsoft.EntityFrameworkCore;
using IsekaiDesnecessario.API.Data;
using IsekaiDesnecessario.API.Models;

namespace IsekaiDesnecessario.API.Services;

public class MissaoService(AppDbContext db, XpService xpService, PontosAtributoService pontosService, ILogger<MissaoService> logger)
{
    // Conclui a ativação da missão e as secundárias vinculadas (deste perfil);
    // credita XP e moedas e registra tudo no diário. Retorna o perfil atualizado.
    public async Task<Perfil?> ConcluirAsync(PerfilMissao ativacao)
    {
        // Operação multi-tabela (ativação + secundárias + XP + moedas + diário):
        // ou tudo num único commit, ou nada (SKILL.md §3 — transações explícitas).
        var def = ativacao.Missao ?? await db.Missoes.FindAsync(ativacao.MissaoId);
        logger.LogInformation("Concluindo missão {MissaoId} ({Titulo}) para perfil {PerfilId}",
            ativacao.MissaoId, def?.Titulo, ativacao.PerfilId);
        await using var tx = await db.Database.BeginTransactionAsync();

        ativacao.Concluida   = true;
        ativacao.ConcluidaEm = DateTime.UtcNow;
        ativacao.Streak++;
        if (def?.TravaDias > 0) ativacao.TravadoAte = DateTime.UtcNow.AddDays(def.TravaDias);

        // Secundárias vinculadas a esta principal (definições) que este perfil tem ativas e abertas
        var secundariasIds = await db.Missoes
            .Where(m => m.MissaoPrincipalId == ativacao.MissaoId)
            .Select(m => m.Id).ToListAsync();
        var vinculadas = await db.PerfilMissoes
            .Include(a => a.Missao)
            .Where(a => a.PerfilId == ativacao.PerfilId && secundariasIds.Contains(a.MissaoId)
                     && a.Ativo && !a.Concluida)
            .ToListAsync();
        foreach (var v in vinculadas)
        {
            v.Concluida   = true;
            v.ConcluidaEm = DateTime.UtcNow;
            v.Streak++;
        }

        await db.SaveChangesAsync();

        await xpService.AdicionarXpAsync(ativacao.PerfilId, def?.RecompensaXp ?? 0);
        await pontosService.IncrementarAsync(ativacao.PerfilId, def?.AtributoId, def?.RecompensaXp ?? 0);
        foreach (var v in vinculadas)
        {
            await xpService.AdicionarXpAsync(ativacao.PerfilId, v.Missao?.RecompensaXp ?? 0);
            await pontosService.IncrementarAsync(ativacao.PerfilId, v.Missao?.AtributoId, v.Missao?.RecompensaXp ?? 0);
        }

        var perfil = await db.Perfis.FindAsync(ativacao.PerfilId);
        if (perfil is not null)
        {
            perfil.Moedas += def?.RecompensaMoedas ?? 0;
            foreach (var v in vinculadas)
                perfil.Moedas += v.Missao?.RecompensaMoedas ?? 0;
        }

        db.DiarioAcoes.Add(new DiarioAcao { PerfilId = ativacao.PerfilId, Emoji = "⚔️", Tipo = "missao",
            Mensagem = $"Concluiu missão \"{def?.Titulo}\" +{def?.RecompensaXp ?? 0} XP +{def?.RecompensaMoedas ?? 0} moedas" });
        foreach (var v in vinculadas)
            db.DiarioAcoes.Add(new DiarioAcao { PerfilId = ativacao.PerfilId, Emoji = "⚔️", Tipo = "missao",
                Mensagem = $"Missão vinculada \"{v.Missao?.Titulo}\" concluída +{v.Missao?.RecompensaXp ?? 0} XP +{v.Missao?.RecompensaMoedas ?? 0} moedas" });

        await db.SaveChangesAsync();
        await tx.CommitAsync();

        logger.LogInformation("Missão {MissaoId} concluída para perfil {PerfilId} ({Secundarias} secundárias)",
            ativacao.MissaoId, ativacao.PerfilId, vinculadas.Count);
        return perfil;
    }
}
