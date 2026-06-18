using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IsekaiDesnecessario.API.Data;
using IsekaiDesnecessario.API.Models;
using IsekaiDesnecessario.API.Services;

namespace IsekaiDesnecessario.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HabitosController(AppDbContext db, XpService xpService) : ApiControllerBase
{
    // --- Bons hábitos ---

    [HttpGet("bons")]
    public async Task<IActionResult> GetBons([FromQuery] int perfilId)
    {
        if (await GarantirDonoDoPerfilAsync(db, perfilId) is { } erro) return erro;
        return Ok(await db.BonsHabitos.Where(h => h.PerfilId == perfilId).AsNoTracking().ToListAsync());
    }

    [HttpPost("bons")]
    public async Task<IActionResult> CreateBom(CriarHabitoDto dto)
    {
        if (await GarantirDonoDoPerfilAsync(db, dto.PerfilId) is { } erro) return erro;
        var habito = new BomHabito
        {
            PerfilId   = dto.PerfilId,
            Habito     = dto.Habito,
            Xp         = dto.Xp,
            Frequencia = dto.Frequencia,
            AtributoId = dto.AtributoId,
        };
        db.BonsHabitos.Add(habito);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetBons), new { perfilId = habito.PerfilId }, habito);
    }

    [HttpPut("bons/{id}")]
    public async Task<IActionResult> UpdateBom(int id, BomHabito habito)
    {
        var h = await db.BonsHabitos.FindAsync(id);
        if (h is null) return NotFound();
        if (await GarantirDonoDoPerfilAsync(db, h.PerfilId) is { } erro) return erro;
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
        if (await GarantirDonoDoPerfilAsync(db, h.PerfilId) is { } erro) return erro;
        db.BonsHabitos.Remove(h);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("bons/{id}/completar")]
    public async Task<IActionResult> CompletarBom(int id, [FromQuery] int perfilId)
    {
        var habito = await db.BonsHabitos.FindAsync(id);
        if (habito is null) return NotFound();
        if (await GarantirDonoDoPerfilAsync(db, habito.PerfilId) is { } erro) return erro;
        if (habito.PerfilId != perfilId) return Forbid();

        // Livre pode sempre completar; outros só se disponível
        if (habito.Frequencia != "Livre" && !EstaDisponivel(habito.Frequencia, habito.UltimaExecucao))
            return BadRequest("Hábito ainda em cooldown.");

        habito.Streak++;
        if (habito.Frequencia != "Livre")
            habito.UltimaExecucao = DateTime.UtcNow;

        await xpService.AdicionarXpAsync(perfilId, habito.Xp);

        db.DiarioAcoes.Add(new() { PerfilId = perfilId, Emoji = "✅", Tipo = "habito_bom",
            Mensagem = $"Completou \"{habito.Habito}\" +{habito.Xp} XP" });

        await db.SaveChangesAsync();
        return Ok(habito);
    }

    // --- Maus hábitos ---

    [HttpGet("maus")]
    public async Task<IActionResult> GetMaus([FromQuery] int perfilId)
    {
        if (await GarantirDonoDoPerfilAsync(db, perfilId) is { } erro) return erro;
        return Ok(await db.MausHabitos.Where(h => h.PerfilId == perfilId).AsNoTracking().ToListAsync());
    }

    [HttpPost("maus")]
    public async Task<IActionResult> CreateMau(CriarHabitoDto dto)
    {
        if (await GarantirDonoDoPerfilAsync(db, dto.PerfilId) is { } erro) return erro;
        var habito = new MauHabito
        {
            PerfilId   = dto.PerfilId,
            Habito     = dto.Habito,
            Xp         = dto.Xp,
            Frequencia = dto.Frequencia,
            AtributoId = dto.AtributoId,
        };
        db.MausHabitos.Add(habito);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetMaus), new { perfilId = habito.PerfilId }, habito);
    }

    [HttpPut("maus/{id}")]
    public async Task<IActionResult> UpdateMau(int id, MauHabito habito)
    {
        var h = await db.MausHabitos.FindAsync(id);
        if (h is null) return NotFound();
        if (await GarantirDonoDoPerfilAsync(db, h.PerfilId) is { } erro) return erro;
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
        if (await GarantirDonoDoPerfilAsync(db, h.PerfilId) is { } erro) return erro;
        db.MausHabitos.Remove(h);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("maus/{id}/registrar")]
    public async Task<IActionResult> RegistrarMau(int id, [FromQuery] int perfilId)
    {
        var habito = await db.MausHabitos.FindAsync(id);
        if (habito is null) return NotFound();
        if (await GarantirDonoDoPerfilAsync(db, habito.PerfilId) is { } erro) return erro;
        if (habito.PerfilId != perfilId) return Forbid();

        if (habito.Frequencia != "Livre" && !EstaDisponivel(habito.Frequencia, habito.UltimaExecucao))
            return BadRequest("Hábito ainda em cooldown.");

        habito.Streak++;
        if (habito.Frequencia != "Livre")
            habito.UltimaExecucao = DateTime.UtcNow;

        db.DiarioAcoes.Add(new() { PerfilId = perfilId, Emoji = "❌", Tipo = "habito_mau",
            Mensagem = $"Registrou \"{habito.Habito}\" -{habito.Xp} XP" });

        await db.SaveChangesAsync();
        await xpService.DeduzerXpAsync(perfilId, habito.Xp);

        var perfil = await db.Perfis.FindAsync(perfilId);
        return Ok(new { habito, perfil });
    }

    // ── Helper: verifica se o hábito já pode ser executado de novo ──
    private static bool EstaDisponivel(string frequencia, DateTime? ultimaExecucao)
    {
        if (ultimaExecucao is null) return true;
        var ultima = ultimaExecucao.Value;
        var agora  = DateTime.UtcNow;

        return frequencia switch
        {
            "Diário"  => ultima.Date < agora.Date,
            "Semanal" => ultima < InicioSemanaAtual(agora),
            "Mensal"  => ultima < new DateTime(agora.Year, agora.Month, 1),
            _         => true
        };
    }

    // Retorna 00:00 do domingo desta semana
    private static DateTime InicioSemanaAtual(DateTime referencia)
    {
        int diff = (int)referencia.DayOfWeek; // 0=Dom
        return referencia.Date.AddDays(-diff);
    }
}

// Streak/UltimaExecucao nunca vêm do cliente — começam zerados/nulos no servidor.
public record CriarHabitoDto(int PerfilId, string Habito, int Xp, string Frequencia, int? AtributoId);
