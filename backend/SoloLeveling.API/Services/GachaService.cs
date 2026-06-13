using Microsoft.EntityFrameworkCore;
using SoloLeveling.API.Data;
using SoloLeveling.API.Models;

namespace SoloLeveling.API.Services;

public class GachaService(AppDbContext db)
{
    // Gacha disponível a cada 3 dias com 300 XP acumulados
    public async Task<Recompensa?> Sortear(int perfilId)
    {
        var perfil = await db.Perfis.FindAsync(perfilId)
            ?? throw new KeyNotFoundException("Perfil não encontrado.");

        var recompensas = await db.Recompensas.ToListAsync();
        if (recompensas.Count == 0) return null;

        var sorteada = recompensas[Random.Shared.Next(recompensas.Count)];
        return sorteada;
    }
}
