using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoloLeveling.API.Data;
using SoloLeveling.API.Models;

namespace SoloLeveling.API.Controllers;

[ApiController]
[Route("api/perfil/{perfilId}/historico")]
public class HistoricoController(AppDbContext db) : ControllerBase
{
    // GET api/perfil/{perfilId}/historico
    [HttpGet]
    public async Task<IActionResult> Get(int perfilId)
    {
        var hist = await db.HistoricoXp
            .Where(h => h.PerfilId == perfilId)
            .OrderBy(h => h.Data)
            .ToListAsync();

        // Retorna sem o campo Json interno
        var resultado = hist.Select(h => new
        {
            h.Id,
            h.PerfilId,
            date   = h.Data.ToString("yyyy-MM-dd"),
            h.XpHoje,
            h.Nivel,
            h.Moedas,
            xpPorHora = h.XpPorHora,
        });

        return Ok(resultado);
    }

    // POST api/perfil/{perfilId}/historico/upsert
    [HttpPost("upsert")]
    public async Task<IActionResult> Upsert(int perfilId, [FromBody] UpsertHistoricoDto dto)
    {
        var hoje = DateOnly.FromDateTime(DateTime.Today);

        var entrada = await db.HistoricoXp
            .FirstOrDefaultAsync(h => h.PerfilId == perfilId && h.Data == hoje);

        if (entrada is null)
        {
            entrada = new HistoricoXp
            {
                PerfilId = perfilId,
                Data     = hoje,
            };
            db.HistoricoXp.Add(entrada);
        }

        entrada.XpHoje = dto.XpHoje;
        entrada.Nivel  = dto.Nivel;
        entrada.Moedas = dto.Moedas;

        // Atualiza a hora atual no array de 24h
        var horas = entrada.XpPorHora;
        horas[dto.Hora] = dto.XpHoje;
        // Propaga para horas anteriores que estejam zeradas
        for (int h = dto.Hora - 1; h >= 0; h--)
            if (horas[h] == 0) horas[h] = 0; // mantém zero antes do primeiro registro
        entrada.XpPorHora = horas;

        await db.SaveChangesAsync();
        return Ok(entrada);
    }
}

public record UpsertHistoricoDto(int XpHoje, int Nivel, int Moedas, int Hora);
