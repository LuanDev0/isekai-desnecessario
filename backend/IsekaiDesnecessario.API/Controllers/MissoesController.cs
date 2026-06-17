using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IsekaiDesnecessario.API.Data;
using IsekaiDesnecessario.API.Models;
using IsekaiDesnecessario.API.Services;

namespace IsekaiDesnecessario.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MissoesController(AppDbContext db, XpService xpService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int perfilId) =>
        Ok(await db.Missoes.Include(m => m.Tipo)
            .Where(m => m.PerfilId == perfilId)
            .OrderBy(m => m.TipoId)
            .ToListAsync());

    [HttpGet("tipos")]
    public async Task<IActionResult> GetTipos() =>
        Ok(await db.TiposMissao.ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create(Missao missao)
    {
        db.Missoes.Add(missao);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), missao);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Missao missao)
    {
        var m = await db.Missoes.FindAsync(id);
        if (m is null) return NotFound();
        m.Titulo = missao.Titulo;
        m.TipoId = missao.TipoId;
        m.RecompensaXp = missao.RecompensaXp;
        m.RecompensaMoedas = missao.RecompensaMoedas;
        m.Concluida = missao.Concluida;
        m.AtributoId = missao.AtributoId;
        m.DataLimite = missao.DataLimite;
        m.MissaoPrincipalId = missao.MissaoPrincipalId;
        await db.SaveChangesAsync();
        return Ok(m);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var missao = await db.Missoes.FindAsync(id);
        if (missao is null) return NotFound();
        db.Missoes.Remove(missao);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/completar")]
    public async Task<IActionResult> Completar(int id, [FromQuery] int perfilId)
    {
        var missao = await db.Missoes.FindAsync(id);
        if (missao is null) return NotFound();
        if (missao.Concluida) return BadRequest("Missão já concluída.");

        missao.Concluida = true;
        missao.ConcluidaEm = DateTime.UtcNow;
        missao.Streak++;

        // Completa missões secundárias vinculadas a esta
        var vinculadas = await db.Missoes
            .Where(m => m.MissaoPrincipalId == id && !m.Concluida)
            .ToListAsync();
        foreach (var v in vinculadas)
        {
            v.Concluida = true;
            v.ConcluidaEm = DateTime.UtcNow;
            v.Streak++;
        }

        await db.SaveChangesAsync();

        await xpService.AdicionarXp(perfilId, missao.RecompensaXp);
        foreach (var v in vinculadas)
            await xpService.AdicionarXp(perfilId, v.RecompensaXp);

        var perfil = await db.Perfis.FindAsync(perfilId);
        if (perfil is not null)
        {
            perfil.Moedas += missao.RecompensaMoedas;
            foreach (var v in vinculadas)
                perfil.Moedas += v.RecompensaMoedas;
        }

        db.DiarioAcoes.Add(new() { PerfilId = perfilId, Emoji = "⚔️", Tipo = "missao",
            Mensagem = $"Concluiu missão \"{missao.Titulo}\" +{missao.RecompensaXp} XP +{missao.RecompensaMoedas} moedas" });
        foreach (var v in vinculadas)
            db.DiarioAcoes.Add(new() { PerfilId = perfilId, Emoji = "⚔️", Tipo = "missao",
                Mensagem = $"Missão vinculada \"{v.Titulo}\" concluída +{v.RecompensaXp} XP +{v.RecompensaMoedas} moedas" });

        await db.SaveChangesAsync();
        return Ok(new { missao, perfil });
    }

    // GET /api/missoes/jornada?perfilId=1  — últimas 12 semanas agrupadas
    [HttpGet("jornada")]
    public async Task<IActionResult> Jornada([FromQuery] int perfilId)
    {
        var doze = DateTime.UtcNow.Date.AddDays(-84); // 12 semanas atrás

        var concluidas = await db.Missoes
            .Where(m => m.PerfilId == perfilId && m.Concluida && m.ConcluidaEm >= doze)
            .Select(m => new { m.ConcluidaEm, m.RecompensaXp, m.TipoId })
            .ToListAsync();

        // Agrupa por início da semana (segunda-feira)
        var grupos = concluidas
            .GroupBy(m => {
                var d = m.ConcluidaEm!.Value.Date;
                int diff = (int)d.DayOfWeek - (int)DayOfWeek.Monday;
                if (diff < 0) diff += 7;
                return d.AddDays(-diff);
            })
            .Select(g => new {
                semana    = g.Key.ToString("yyyy-MM-dd"),
                total     = g.Count(),
                xp        = g.Sum(m => m.RecompensaXp),
                principais = g.Count(m => m.TipoId == 1),
            })
            .OrderBy(g => g.semana)
            .ToList();

        return Ok(grupos);
    }

    [HttpPost("{id}/resetar")]
    public async Task<IActionResult> Resetar(int id)
    {
        var missao = await db.Missoes.FindAsync(id);
        if (missao is null) return NotFound();
        missao.Concluida = false;
        await db.SaveChangesAsync();
        return Ok(missao);
    }
}
