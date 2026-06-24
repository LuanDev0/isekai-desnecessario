using IsekaiDesnecessario.API.Data;
using IsekaiDesnecessario.API.Models;

namespace IsekaiDesnecessario.API.Services;

public class PontosAtributoService(AppDbContext db)
{
    public async Task IncrementarAsync(int perfilId, int? atributoId, int xp)
    {
        if (atributoId is null || xp <= 0) return;
        int ganho = xp / 10;
        if (ganho <= 0) return;

        var row = await db.PontosAtributos.FindAsync(perfilId, atributoId.Value);
        if (row is null)
            db.PontosAtributos.Add(new PontosAtributo { PerfilId = perfilId, AtributoId = atributoId.Value, Total = ganho });
        else
            row.Total += ganho;
    }
}
