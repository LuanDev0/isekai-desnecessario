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
    // ════════════════════ Bons hábitos ════════════════════

    // Hábitos ativos no perfil (achatado: definição + estado da ativação)
    [HttpGet("bons")]
    public async Task<IActionResult> GetBons([FromQuery] int perfilId)
    {
        if (await GarantirDonoDoPerfilAsync(db, perfilId) is { } erro) return erro;
        var lista = await db.PerfilBonsHabitos
            .Where(a => a.PerfilId == perfilId && a.Ativo)
            .Select(a => new HabitoDto(a.BomHabito!.Id, perfilId, a.BomHabito.Habito, a.BomHabito.Xp,
                a.BomHabito.Frequencia, a.Streak, a.UltimaExecucao, a.BomHabito.AtributoId))
            .AsNoTracking().ToListAsync();
        return Ok(lista);
    }

    // Catálogo de bons hábitos disponíveis ao perfil (aprovados: globais + próprios)
    [HttpGet("bons/catalogo")]
    public async Task<IActionResult> CatalogoBons([FromQuery] int perfilId)
    {
        if (await GarantirDonoDoPerfilAsync(db, perfilId) is { } erro) return erro;
        var uid = UsuarioId;
        var lista = await db.BonsHabitos
            .Where(d => d.Status == StatusConteudo.Aprovado
                     && (d.Escopo == EscopoConteudo.Global || d.CriadoPorUsuarioId == uid))
            .Select(d => new HabitoCatalogoDto(d.Id, d.Habito, d.Xp, d.Frequencia, d.AtributoId, d.Escopo, d.Status,
                db.PerfilBonsHabitos.Any(a => a.PerfilId == perfilId && a.BomHabitoId == d.Id && a.Ativo)))
            .AsNoTracking().ToListAsync();
        return Ok(lista);
    }

    [HttpPost("bons")]
    public async Task<IActionResult> CreateBom(CriarHabitoDto dto)
    {
        if (await GarantirDonoDoPerfilAsync(db, dto.PerfilId) is { } erro) return erro;
        var autoria = DefinirAutoria(await ObterRoleAsync(db));
        if (autoria is null)
            return StatusCode(StatusCodes.Status403Forbidden, "Seu papel não pode criar conteúdo.");
        var (escopo, status) = autoria.Value;

        var def = new BomHabito
        {
            Habito = dto.Habito, Xp = dto.Xp, Frequencia = dto.Frequencia, AtributoId = dto.AtributoId,
            Escopo = escopo, Status = status, CriadoPorUsuarioId = UsuarioId,
        };
        // Auto-ativa para o perfil que criou (mantém UX atual: criou → aparece)
        var ativacao = new PerfilBomHabito { PerfilId = dto.PerfilId, Ativo = true };
        def.Ativacoes.Add(ativacao);
        db.BonsHabitos.Add(def);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetBons), new { perfilId = dto.PerfilId },
            new HabitoDto(def.Id, dto.PerfilId, def.Habito, def.Xp, def.Frequencia, 0, null, def.AtributoId));
    }

    [HttpPut("bons/{id}")]
    public async Task<IActionResult> UpdateBom(int id, BomHabito habito)
    {
        var d = await db.BonsHabitos.FindAsync(id);
        if (d is null) return NotFound();
        if (!await PodeMutarDefAsync(d.CriadoPorUsuarioId)) return Forbid();
        d.Habito = habito.Habito;
        d.Xp = habito.Xp;
        d.Frequencia = habito.Frequencia;
        d.AtributoId = habito.AtributoId;
        await db.SaveChangesAsync();
        return Ok(d);
    }

    [HttpDelete("bons/{id}")]
    public async Task<IActionResult> DeleteBom(int id)
    {
        var d = await db.BonsHabitos.FindAsync(id);
        if (d is null) return NotFound();
        if (!await PodeMutarDefAsync(d.CriadoPorUsuarioId)) return Forbid();
        db.BonsHabitos.Remove(d); // ativações caem em cascata
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("bons/{id}/ativar")]
    public async Task<IActionResult> AtivarBom(int id, [FromQuery] int perfilId) =>
        await AtivarAsync(id, perfilId, ativar: true, bom: true);

    [HttpPost("bons/{id}/desativar")]
    public async Task<IActionResult> DesativarBom(int id, [FromQuery] int perfilId) =>
        await AtivarAsync(id, perfilId, ativar: false, bom: true);

    [HttpPost("bons/{id}/completar")]
    public async Task<IActionResult> CompletarBom(int id, [FromQuery] int perfilId)
    {
        if (await GarantirDonoDoPerfilAsync(db, perfilId) is { } erro) return erro;
        var a = await db.PerfilBonsHabitos.Include(x => x.BomHabito)
            .FirstOrDefaultAsync(x => x.PerfilId == perfilId && x.BomHabitoId == id && x.Ativo);
        if (a?.BomHabito is null) return NotFound();
        var def = a.BomHabito;

        if (def.Frequencia != "Livre" && !EstaDisponivel(def.Frequencia, a.UltimaExecucao))
            return BadRequest("Hábito ainda em cooldown.");

        a.Streak++;
        if (def.Frequencia != "Livre") a.UltimaExecucao = DateTime.UtcNow;

        await xpService.AdicionarXpAsync(perfilId, def.Xp);
        db.DiarioAcoes.Add(new() { PerfilId = perfilId, Emoji = "✅", Tipo = "habito_bom",
            Mensagem = $"Completou \"{def.Habito}\" +{def.Xp} XP" });
        await db.SaveChangesAsync();

        return Ok(new HabitoDto(def.Id, perfilId, def.Habito, def.Xp, def.Frequencia, a.Streak, a.UltimaExecucao, def.AtributoId));
    }

    // ════════════════════ Maus hábitos ════════════════════

    [HttpGet("maus")]
    public async Task<IActionResult> GetMaus([FromQuery] int perfilId)
    {
        if (await GarantirDonoDoPerfilAsync(db, perfilId) is { } erro) return erro;
        var lista = await db.PerfilMausHabitos
            .Where(a => a.PerfilId == perfilId && a.Ativo)
            .Select(a => new HabitoDto(a.MauHabito!.Id, perfilId, a.MauHabito.Habito, a.MauHabito.Xp,
                a.MauHabito.Frequencia, a.Streak, a.UltimaExecucao, a.MauHabito.AtributoId))
            .AsNoTracking().ToListAsync();
        return Ok(lista);
    }

    [HttpGet("maus/catalogo")]
    public async Task<IActionResult> CatalogoMaus([FromQuery] int perfilId)
    {
        if (await GarantirDonoDoPerfilAsync(db, perfilId) is { } erro) return erro;
        var uid = UsuarioId;
        var lista = await db.MausHabitos
            .Where(d => d.Status == StatusConteudo.Aprovado
                     && (d.Escopo == EscopoConteudo.Global || d.CriadoPorUsuarioId == uid))
            .Select(d => new HabitoCatalogoDto(d.Id, d.Habito, d.Xp, d.Frequencia, d.AtributoId, d.Escopo, d.Status,
                db.PerfilMausHabitos.Any(a => a.PerfilId == perfilId && a.MauHabitoId == d.Id && a.Ativo)))
            .AsNoTracking().ToListAsync();
        return Ok(lista);
    }

    [HttpPost("maus")]
    public async Task<IActionResult> CreateMau(CriarHabitoDto dto)
    {
        if (await GarantirDonoDoPerfilAsync(db, dto.PerfilId) is { } erro) return erro;
        var autoria = DefinirAutoria(await ObterRoleAsync(db));
        if (autoria is null)
            return StatusCode(StatusCodes.Status403Forbidden, "Seu papel não pode criar conteúdo.");
        var (escopo, status) = autoria.Value;

        var def = new MauHabito
        {
            Habito = dto.Habito, Xp = dto.Xp, Frequencia = dto.Frequencia, AtributoId = dto.AtributoId,
            Escopo = escopo, Status = status, CriadoPorUsuarioId = UsuarioId,
        };
        def.Ativacoes.Add(new PerfilMauHabito { PerfilId = dto.PerfilId, Ativo = true });
        db.MausHabitos.Add(def);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetMaus), new { perfilId = dto.PerfilId },
            new HabitoDto(def.Id, dto.PerfilId, def.Habito, def.Xp, def.Frequencia, 0, null, def.AtributoId));
    }

    [HttpPut("maus/{id}")]
    public async Task<IActionResult> UpdateMau(int id, MauHabito habito)
    {
        var d = await db.MausHabitos.FindAsync(id);
        if (d is null) return NotFound();
        if (!await PodeMutarDefAsync(d.CriadoPorUsuarioId)) return Forbid();
        d.Habito = habito.Habito;
        d.Xp = habito.Xp;
        d.Frequencia = habito.Frequencia;
        d.AtributoId = habito.AtributoId;
        await db.SaveChangesAsync();
        return Ok(d);
    }

    [HttpDelete("maus/{id}")]
    public async Task<IActionResult> DeleteMau(int id)
    {
        var d = await db.MausHabitos.FindAsync(id);
        if (d is null) return NotFound();
        if (!await PodeMutarDefAsync(d.CriadoPorUsuarioId)) return Forbid();
        db.MausHabitos.Remove(d);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("maus/{id}/ativar")]
    public async Task<IActionResult> AtivarMau(int id, [FromQuery] int perfilId) =>
        await AtivarAsync(id, perfilId, ativar: true, bom: false);

    [HttpPost("maus/{id}/desativar")]
    public async Task<IActionResult> DesativarMau(int id, [FromQuery] int perfilId) =>
        await AtivarAsync(id, perfilId, ativar: false, bom: false);

    [HttpPost("maus/{id}/registrar")]
    public async Task<IActionResult> RegistrarMau(int id, [FromQuery] int perfilId)
    {
        if (await GarantirDonoDoPerfilAsync(db, perfilId) is { } erro) return erro;
        var a = await db.PerfilMausHabitos.Include(x => x.MauHabito)
            .FirstOrDefaultAsync(x => x.PerfilId == perfilId && x.MauHabitoId == id && x.Ativo);
        if (a?.MauHabito is null) return NotFound();
        var def = a.MauHabito;

        if (def.Frequencia != "Livre" && !EstaDisponivel(def.Frequencia, a.UltimaExecucao))
            return BadRequest("Hábito ainda em cooldown.");

        a.Streak++;
        if (def.Frequencia != "Livre") a.UltimaExecucao = DateTime.UtcNow;

        db.DiarioAcoes.Add(new() { PerfilId = perfilId, Emoji = "❌", Tipo = "habito_mau",
            Mensagem = $"Registrou \"{def.Habito}\" -{def.Xp} XP" });
        await db.SaveChangesAsync();
        await xpService.DeduzerXpAsync(perfilId, def.Xp);

        var habitoDto = new HabitoDto(def.Id, perfilId, def.Habito, def.Xp, def.Frequencia, a.Streak, a.UltimaExecucao, def.AtributoId);
        var perfil = await db.Perfis.FindAsync(perfilId);
        return Ok(new { habito = habitoDto, perfil });
    }

    // ── Ativar/desativar genérico (bom/mau) ──────────────────────
    private async Task<IActionResult> AtivarAsync(int defId, int perfilId, bool ativar, bool bom)
    {
        if (await GarantirDonoDoPerfilAsync(db, perfilId) is { } erro) return erro;
        var uid = UsuarioId;

        if (bom)
        {
            var visivel = await db.BonsHabitos.AnyAsync(d => d.Id == defId
                && d.Status == StatusConteudo.Aprovado
                && (d.Escopo == EscopoConteudo.Global || d.CriadoPorUsuarioId == uid));
            if (!visivel) return NotFound();
            var a = await db.PerfilBonsHabitos.FirstOrDefaultAsync(x => x.PerfilId == perfilId && x.BomHabitoId == defId);
            if (a is null) db.PerfilBonsHabitos.Add(new() { PerfilId = perfilId, BomHabitoId = defId, Ativo = ativar });
            else a.Ativo = ativar;
        }
        else
        {
            var visivel = await db.MausHabitos.AnyAsync(d => d.Id == defId
                && d.Status == StatusConteudo.Aprovado
                && (d.Escopo == EscopoConteudo.Global || d.CriadoPorUsuarioId == uid));
            if (!visivel) return NotFound();
            var a = await db.PerfilMausHabitos.FirstOrDefaultAsync(x => x.PerfilId == perfilId && x.MauHabitoId == defId);
            if (a is null) db.PerfilMausHabitos.Add(new() { PerfilId = perfilId, MauHabitoId = defId, Ativo = ativar });
            else a.Ativo = ativar;
        }
        await db.SaveChangesAsync();
        return Ok();
    }

    // Pode editar/excluir a definição: o autor ou um Admin.
    private async Task<bool> PodeMutarDefAsync(int? autorId)
    {
        if (UsuarioId is not int uid) return false;
        if (autorId == uid) return true;
        return await ObterRoleAsync(db) == Role.Admin;
    }

    // ── Cooldown por frequência (reaproveitado da versão anterior) ──
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

    private static DateTime InicioSemanaAtual(DateTime referencia)
    {
        int diff = (int)referencia.DayOfWeek; // 0=Dom
        return referencia.Date.AddDays(-diff);
    }
}

// Streak/UltimaExecucao nunca vêm do cliente — começam zerados/nulos no servidor.
public record CriarHabitoDto(int PerfilId, string Habito, int Xp, string Frequencia, int? AtributoId);

// Item ativo no perfil (definição achatada + estado da ativação).
public record HabitoDto(int Id, int PerfilId, string Habito, int Xp, string Frequencia,
    int Streak, DateTime? UltimaExecucao, int? AtributoId);

// Item do catálogo (definição + se já está ativo no perfil).
public record HabitoCatalogoDto(int Id, string Habito, int Xp, string Frequencia, int? AtributoId,
    EscopoConteudo Escopo, StatusConteudo Status, bool Ativo);
