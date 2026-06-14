using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IsekaiDesnecessario.API.Data;
using IsekaiDesnecessario.API.Models;
using IsekaiDesnecessario.API.Services;

namespace IsekaiDesnecessario.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HabitosController(AppDbContext db, XpService xpService) : ControllerBase
{
    // --- Bons hábitos ---

    [HttpGet("bons")]
    public async Task<IActionResult> GetBons([FromQuery] int perfilId) =>
        Ok(await db.BonsHabitos.Where(h => h.PerfilId == perfilId).ToListAsync());

    [HttpPost("bons")]
    public async Task<IActionResult> CreateBom(BomHabito habito)
    {
        db.BonsHabitos.Add(habito);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetBons), habito);
    }

    [HttpPut("bons/{id}")]
    public async Task<IActionResult> UpdateBom(int id, BomHabito habito)
    {
        var h = await db.BonsHabitos.FindAsync(id);
        if (h is null) return NotFound();
        h.Habito = habito.Habito;
        h.Xp = habito.Xp;
        h.Frequencia = habito.Frequencia;
        h.AtributoId = habito.AtributoId;
        await db.SaveChangesAsync();
        return Ok(h);
    }

    [HttpDelete("bons/{id}")]
    public async Task<IActionResult> DeleteBom(int id)
    {
        var h = await db.BonsHabitos.FindAsync(id);
        if (h is null) return NotFound();
        db.BonsHabitos.Remove(h);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("bons/{id}/completar")]
    public async Task<IActionResult> CompletarBom(int id, [FromQuery] int perfilId)
    {
        var habito = await db.BonsHabitos.FindAsync(id);
        if (habito is null) return NotFound();

        // Livre pode sempre completar; outros só se disponível
        if (habito.Frequencia != "Livre" && !EstaDisponivel(habito.Frequencia, habito.UltimaExecucao))
            return BadRequest("Hábito ainda em cooldown.");

        habito.Streak++;
        if (habito.Frequencia != "Livre")
            habito.UltimaExecucao = DateTime.Now;

        await xpService.AdicionarXp(perfilId, habito.Xp);

        db.DiarioAcoes.Add(new() { PerfilId = perfilId, Emoji = "✅", Tipo = "habito_bom",
            Mensagem = $"Completou \"{habito.Habito}\" +{habito.Xp} XP" });

        await db.SaveChangesAsync();
        return Ok(habito);
    }

    // --- Maus hábitos ---

    [HttpGet("maus")]
    public async Task<IActionResult> GetMaus([FromQuery] int perfilId) =>
        Ok(await db.MausHabitos.Where(h => h.PerfilId == perfilId).ToListAsync());

    [HttpPost("maus")]
    public async Task<IActionResult> CreateMau(MauHabito habito)
    {
        db.MausHabitos.Add(habito);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetMaus), habito);
    }

    [HttpPut("maus/{id}")]
    public async Task<IActionResult> UpdateMau(int id, MauHabito habito)
    {
        var h = await db.MausHabitos.FindAsync(id);
        if (h is null) return NotFound();
        h.Habito = habito.Habito;
        h.Xp = habito.Xp;
        h.Frequencia = habito.Frequencia;
        h.AtributoId = habito.AtributoId;
        await db.SaveChangesAsync();
        return Ok(h);
    }

    [HttpDelete("maus/{id}")]
    public async Task<IActionResult> DeleteMau(int id)
    {
        var h = await db.MausHabitos.FindAsync(id);
        if (h is null) return NotFound();
        db.MausHabitos.Remove(h);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("maus/{id}/registrar")]
    public async Task<IActionResult> RegistrarMau(int id, [FromQuery] int perfilId)
    {
        var habito = await db.MausHabitos.FindAsync(id);
        if (habito is null) return NotFound();

        if (habito.Frequencia != "Livre" && !EstaDisponivel(habito.Frequencia, habito.UltimaExecucao))
            return BadRequest("Hábito ainda em cooldown.");

        habito.Streak++;
        if (habito.Frequencia != "Livre")
            habito.UltimaExecucao = DateTime.Now;

        db.DiarioAcoes.Add(new() { PerfilId = perfilId, Emoji = "❌", Tipo = "habito_mau",
            Mensagem = $"Registrou \"{habito.Habito}\" -{habito.Xp} XP" });

        await db.SaveChangesAsync();
        await xpService.DeduzerXp(perfilId, habito.Xp);

        var perfil = await db.Perfis.FindAsync(perfilId);
        return Ok(new { habito, perfil });
    }

    // ── Helper: verifica se o hábito já pode ser executado de novo ──
    private static bool EstaDisponivel(string frequencia, DateTime? ultimaExecucao)
    {
        if (ultimaExecucao is null) return true;
        var ultima = ultimaExecucao.Value;
        var agora  = DateTime.Now;

        return frequencia switch
        {
            "Diário"  => ultima.Date < agora.Date,
            "Semanal" => ultima < InicioSemanaAtual(agora),
            "Mensal"  => ultima < new DateTime(agora.Year, agora.Month, 1),
            _         => true
        };
    }

    // Retorna 00:00 do domingo desta semana
    private static DateTime InicioSemanaAtual(DateTime ref_)
    {
        int diff = (int)ref_.DayOfWeek; // 0=Dom
        return ref_.Date.AddDays(-diff);
    }
}
