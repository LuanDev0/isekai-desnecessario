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
public class MissoesController(AppDbContext db, MissaoService missaoService, NotificacaoService notificacoes) : ApiControllerBase
{
    // Missões ativas no perfil (definição achatada + estado da ativação)
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int perfilId)
    {
        if (await GarantirDonoDoPerfilAsync(db, perfilId) is { } erro) return erro;
        var lista = await db.PerfilMissoes
            .Where(a => a.PerfilId == perfilId && a.Ativo)
            .OrderBy(a => a.Missao!.TipoId)
            .Select(a => new MissaoDto(a.Missao!.Id, perfilId, a.Missao.Titulo, a.Missao.TipoId, a.Missao.Tipo,
                a.Missao.RecompensaXp, a.Missao.RecompensaMoedas, a.Streak, a.Concluida, a.ConcluidaEm,
                a.Missao.DataLimite, a.Missao.MissaoPrincipalId, a.Missao.AtributoId, a.Missao.Escopo))
            .AsNoTracking().ToListAsync();
        return Ok(lista);
    }

    [HttpGet("tipos")]
    public async Task<IActionResult> GetTipos() =>
        Ok(await db.TiposMissao.AsNoTracking().ToListAsync());

    [HttpGet("catalogo")]
    public async Task<IActionResult> Catalogo([FromQuery] int perfilId)
    {
        if (await GarantirDonoDoPerfilAsync(db, perfilId) is { } erro) return erro;
        var uid = UsuarioId;
        var classeId = await ClasseDoPerfilAsync(db, perfilId);
        var lista = await db.Missoes
            .Where(d => d.Status == StatusConteudo.Aprovado
                     && (d.Escopo == EscopoConteudo.Global || d.CriadoPorUsuarioId == uid))
            .Select(d => new MissaoCatalogoDto(d.Id, d.Titulo, d.TipoId, d.Tipo, d.RecompensaXp, d.RecompensaMoedas,
                d.DataLimite, d.MissaoPrincipalId, d.AtributoId, d.Escopo, d.Status,
                db.PerfilMissoes.Any(a => a.PerfilId == perfilId && a.MissaoId == d.Id && a.Ativo),
                d.Classes.Select(c => c.Id).ToArray(),
                d.Classes.Any() && !d.Classes.Any(c => c.Id == classeId)))
            .AsNoTracking().ToListAsync();
        return Ok(lista);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CriarMissaoDto dto)
    {
        if (await GarantirDonoDoPerfilAsync(db, dto.PerfilId) is { } erro) return erro;
        var autoria = DefinirAutoria(await ObterRoleAsync(db), dto.Proprio);
        if (autoria is null)
            return StatusCode(StatusCodes.Status403Forbidden, "Seu papel não pode criar esse conteúdo.");
        var (escopo, status) = autoria.Value;

        var def = new Missao
        {
            Titulo = dto.Titulo, TipoId = dto.TipoId, RecompensaXp = dto.RecompensaXp,
            RecompensaMoedas = dto.RecompensaMoedas, AtributoId = dto.AtributoId,
            DataLimite = dto.DataLimite.HasValue ? DateTime.SpecifyKind(dto.DataLimite.Value, DateTimeKind.Utc) : null,
            MissaoPrincipalId = dto.MissaoPrincipalId,
            TravaDias = dto.TravaDias, Escopo = escopo, Status = status, CriadoPorUsuarioId = UsuarioId,
        };
        if (escopo == EscopoConteudo.Global && dto.ClasseIds is { Count: > 0 })
            def.Classes = await db.Classes.Where(c => dto.ClasseIds.Contains(c.Id)).ToListAsync();

        var classeId = await ClasseDoPerfilAsync(db, dto.PerfilId);
        if (def.Classes.Count == 0 || (classeId != null && def.Classes.Any(c => c.Id == classeId)))
            def.Ativacoes.Add(new PerfilMissao { PerfilId = dto.PerfilId, Ativo = true });
        db.Missoes.Add(def);
        await db.SaveChangesAsync();
        if (status == StatusConteudo.Pendente)
            await notificacoes.NotificarAdminsAsync("pendente", $"Nova missão pendente de aprovação: \"{def.Titulo}\".");
        return CreatedAtAction(nameof(GetAll), new { perfilId = dto.PerfilId },
            new MissaoDto(def.Id, dto.PerfilId, def.Titulo, def.TipoId, null, def.RecompensaXp,
                def.RecompensaMoedas, 0, false, null, def.DataLimite, def.MissaoPrincipalId, def.AtributoId, def.Escopo));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Missao missao)
    {
        var d = await db.Missoes.FindAsync(id);
        if (d is null) return NotFound();
        if (!await PodeMutarDefinicaoAsync(db, d.CriadoPorUsuarioId, d.Escopo, d.Status)) return Forbid();
        d.Titulo = missao.Titulo;
        d.TipoId = missao.TipoId;
        d.RecompensaXp = missao.RecompensaXp;
        d.RecompensaMoedas = missao.RecompensaMoedas;
        d.AtributoId = missao.AtributoId;
        d.DataLimite = missao.DataLimite.HasValue ? DateTime.SpecifyKind(missao.DataLimite.Value, DateTimeKind.Utc) : null;
        d.MissaoPrincipalId = missao.MissaoPrincipalId;
        await db.SaveChangesAsync();
        await AvisarSeAdminModificouAsync(db, notificacoes, d);
        return Ok(d);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var d = await db.Missoes.FindAsync(id);
        if (d is null) return NotFound();
        if (!await PodeMutarDefinicaoAsync(db, d.CriadoPorUsuarioId, d.Escopo, d.Status)) return Forbid();
        db.Missoes.Remove(d); // ativações caem em cascata
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/ativar")]
    public async Task<IActionResult> Ativar(int id, [FromQuery] int perfilId) =>
        await AtivarAsync(id, perfilId, ativar: true);

    [HttpPost("{id}/desativar")]
    public async Task<IActionResult> Desativar(int id, [FromQuery] int perfilId) =>
        await AtivarAsync(id, perfilId, ativar: false);

    [HttpPost("{id}/completar")]
    public async Task<IActionResult> Completar(int id, [FromQuery] int perfilId)
    {
        if (await GarantirDonoDoPerfilAsync(db, perfilId) is { } erro) return erro;
        var a = await db.PerfilMissoes.Include(x => x.Missao)
            .FirstOrDefaultAsync(x => x.PerfilId == perfilId && x.MissaoId == id && x.Ativo);
        if (a?.Missao is null) return NotFound();
        if (a.Concluida) return BadRequest("Missão já concluída.");

        var perfil = await missaoService.ConcluirAsync(a);
        var dto = new MissaoDto(a.Missao.Id, perfilId, a.Missao.Titulo, a.Missao.TipoId, a.Missao.Tipo,
            a.Missao.RecompensaXp, a.Missao.RecompensaMoedas, a.Streak, a.Concluida, a.ConcluidaEm,
            a.Missao.DataLimite, a.Missao.MissaoPrincipalId, a.Missao.AtributoId, a.Missao.Escopo);
        return Ok(new { missao = dto, perfil });
    }

    // GET /api/missoes/jornada?perfilId=1 — últimas 12 semanas agrupadas
    [HttpGet("jornada")]
    public async Task<IActionResult> Jornada([FromQuery] int perfilId)
    {
        if (await GarantirDonoDoPerfilAsync(db, perfilId) is { } erro) return erro;

        var doze = DateTime.UtcNow.Date.AddDays(-84);

        var concluidas = await db.PerfilMissoes
            .Where(a => a.PerfilId == perfilId && a.Concluida && a.ConcluidaEm >= doze)
            .Select(a => new { a.ConcluidaEm, a.Missao!.RecompensaXp, a.Missao.TipoId })
            .ToListAsync();

        var grupos = concluidas
            .GroupBy(m => {
                var d = m.ConcluidaEm!.Value.Date;
                int diff = (int)d.DayOfWeek - (int)DayOfWeek.Monday;
                if (diff < 0) diff += 7;
                return d.AddDays(-diff);
            })
            .Select(g => new {
                semana     = g.Key.ToString("yyyy-MM-dd"),
                total      = g.Count(),
                xp         = g.Sum(m => m.RecompensaXp),
                principais = g.Count(m => m.TipoId == 1),
            })
            .OrderBy(g => g.semana)
            .ToList();

        return Ok(grupos);
    }

    [HttpPost("{id}/resetar")]
    public async Task<IActionResult> Resetar(int id, [FromQuery] int perfilId)
    {
        if (await GarantirDonoDoPerfilAsync(db, perfilId) is { } erro) return erro;
        var a = await db.PerfilMissoes.FirstOrDefaultAsync(x => x.PerfilId == perfilId && x.MissaoId == id);
        if (a is null) return NotFound();
        a.Concluida = false;
        a.ConcluidaEm = null;
        await db.SaveChangesAsync();
        return Ok();
    }

    private async Task<IActionResult> AtivarAsync(int defId, int perfilId, bool ativar)
    {
        if (await GarantirDonoDoPerfilAsync(db, perfilId) is { } erro) return erro;
        var uid = UsuarioId;
        var info = await db.Missoes
            .Where(d => d.Id == defId && d.Status == StatusConteudo.Aprovado
                     && (d.Escopo == EscopoConteudo.Global || d.CriadoPorUsuarioId == uid))
            .Select(d => new { Classes = d.Classes.Select(c => c.Id).ToArray() }).FirstOrDefaultAsync();
        if (info is null) return NotFound();

        if (ativar && info.Classes.Length > 0)
        {
            var classeId = await ClasseDoPerfilAsync(db, perfilId);
            if (classeId is null || !info.Classes.Contains(classeId.Value))
                return BadRequest("Item exclusivo de outra classe.");
        }

        var a = await db.PerfilMissoes.FirstOrDefaultAsync(x => x.PerfilId == perfilId && x.MissaoId == defId);
        if (!ativar && Travado(a?.TravadoAte) is { } travaErro) return travaErro;
        if (a is null) db.PerfilMissoes.Add(new() { PerfilId = perfilId, MissaoId = defId, Ativo = ativar });
        else a.Ativo = ativar;

        // Ao ativar uma missão principal, ativar automaticamente as secundárias vinculadas.
        if (ativar)
        {
            var secundarias = await db.Missoes
                .Where(d => d.MissaoPrincipalId == defId && d.Status == StatusConteudo.Aprovado)
                .Select(d => d.Id)
                .ToListAsync();
            var ativacoesExistentes = await db.PerfilMissoes
                .Where(x => x.PerfilId == perfilId && secundarias.Contains(x.MissaoId))
                .ToListAsync();
            foreach (var secId in secundarias)
            {
                var ativacaoSec = ativacoesExistentes.FirstOrDefault(x => x.MissaoId == secId);
                if (ativacaoSec is null) db.PerfilMissoes.Add(new() { PerfilId = perfilId, MissaoId = secId, Ativo = true });
                else ativacaoSec.Ativo = true;
            }
        }

        await db.SaveChangesAsync();
        return Ok();
    }
}

// Concluida/ConcluidaEm/Streak nunca vêm do cliente — missão nasce "aberta".
// ClasseIds: vínculo multi-classe (só aplicado a conteúdo global).
// Proprio: true = conteúdo privado da conta (VIP/Mod/Admin); false = catálogo global.
public record CriarMissaoDto(
    int PerfilId, string Titulo, int TipoId, int RecompensaXp, int RecompensaMoedas,
    int? AtributoId, DateTime? DataLimite, int? MissaoPrincipalId, List<int>? ClasseIds, bool Proprio,
    int TravaDias);

// Missão ativa no perfil (definição achatada + estado da ativação).
public record MissaoDto(int Id, int PerfilId, string Titulo, int TipoId, TipoMissao? Tipo,
    int RecompensaXp, int RecompensaMoedas, int Streak, bool Concluida, DateTime? ConcluidaEm,
    DateTime? DataLimite, int? MissaoPrincipalId, int? AtributoId, EscopoConteudo Escopo);

// Missão do catálogo (definição + se já está ativa no perfil + classes/bloqueio).
public record MissaoCatalogoDto(int Id, string Titulo, int TipoId, TipoMissao? Tipo,
    int RecompensaXp, int RecompensaMoedas, DateTime? DataLimite, int? MissaoPrincipalId, int? AtributoId,
    EscopoConteudo Escopo, StatusConteudo Status, bool Ativo, int[] ClasseIds, bool Bloqueado);
