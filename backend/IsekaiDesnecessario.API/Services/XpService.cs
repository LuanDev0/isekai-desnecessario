using IsekaiDesnecessario.API.Data;
using IsekaiDesnecessario.API.Models;

namespace IsekaiDesnecessario.API.Services;

public class XpService(AppDbContext db)
{
    // Fator de crescimento do XP necessário para o próximo nível (XP_próximo = round(XP_atual × 1.036))
    private const double FatorProximoNivel = 1.036;

    // Índice = Nivel / 10  →  1-9=H, 10-19=G, 20-29=F, 30-39=E, 40-49=D, 50-59=C, 60-69=B, 70-79=A, 80-89=S, 90-99=SS, 100+=SSS
    private static readonly string[] Ranks  = ["H", "G", "F", "E", "D", "C", "B", "A", "S", "SS", "SSS"];
    private static readonly string[] Titulos = [
        "Iniciante", "Aprendiz", "Aventureiro", "Veterano", "Renomado",
        "Eminente", "Especialista", "Campeão", "Herói", "Mestre",
        "Glorioso", "Radiante", "Supremo", "Lendário", "Divino",
        "Deus Único"
    ];

    public async Task AdicionarXpAsync(int perfilId, int xpGanho)
    {
        var perfil = await db.Perfis.FindAsync(perfilId)
            ?? throw new KeyNotFoundException("Perfil não encontrado.");

        perfil.Xp += xpGanho;

        // Acumula XP do dia (reseta à meia-noite)
        perfil.ResetarXpDiarioSeNovoDia();
        perfil.XpHoje += xpGanho;

        // Subiu de nível
        while (perfil.Xp >= perfil.ProximoNivelXp)
        {
            perfil.Xp -= perfil.ProximoNivelXp;
            perfil.Nivel++;
            perfil.ProximoNivelXp = (int)Math.Round(perfil.ProximoNivelXp * FatorProximoNivel);
            AtualizarRank(perfil);

            db.DiarioAcoes.Add(new Models.DiarioAcao { PerfilId = perfilId, Emoji = "🎉", Tipo = "nivel",
                Mensagem = $"Subiu para Nível {perfil.Nivel}! Rank {perfil.Rank}" });
        }

        await db.SaveChangesAsync();
    }

    public async Task DeduzerXpAsync(int perfilId, int penalidade)
    {
        var perfil = await db.Perfis.FindAsync(perfilId)
            ?? throw new KeyNotFoundException("Perfil não encontrado.");

        perfil.Xp -= penalidade;

        // Regredir níveis enquanto XP for negativo e nível > 1
        while (perfil.Xp < 0 && perfil.Nivel > 1)
        {
            // Recalcula o ProximoNivelXp do nível anterior
            // (inverte a fórmula: xpAnterior = round(xpAtual / 1.036))
            int xpNivelAnterior = (int)Math.Round(perfil.ProximoNivelXp / FatorProximoNivel);

            perfil.Nivel--;
            perfil.ProximoNivelXp = xpNivelAnterior;

            // O XP negativo "consome" o nível anterior a partir do fim
            // ex: estava com -10 → vira xpNivelAnterior - 10
            perfil.Xp += xpNivelAnterior;
            AtualizarRank(perfil);
        }

        // Nível 1 não vai abaixo de 0
        if (perfil.Xp < 0)
            perfil.Xp = 0;

        await db.SaveChangesAsync();
    }

    private static void AtualizarRank(Perfil perfil)
    {
        int index = perfil.Nivel / 10;
        perfil.Rank   = Ranks  [Math.Min(index, Ranks.Length   - 1)];
        perfil.Titulo = Titulos[Math.Min(index, Titulos.Length - 1)];
    }
}
