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
public class PerfilController(AppDbContext db, XpService xpService, LootboxService lootbox) : ApiControllerBase
{
    // Máximo de heróis (perfis) que uma conta Google pode ter
    public const int MaxPerfisPorConta = 3;

    [HttpGet("ranking")]
    public async Task<IActionResult> GetRanking([FromQuery] int top = 50)
    {
        var limite = Math.Clamp(top, 1, 100);
        var perfis = await db.Perfis
            .AsNoTracking()
            .Where(p => p.Principal)
            .OrderByDescending(p => p.Nivel).ThenByDescending(p => p.Xp)
            .Take(limite)
            .Select(p => new {
                p.Id,
                p.Nome,
                p.Nivel,
                p.Xp,
                p.Rank,
                p.Titulo,
                p.FotoUrl,
                p.ClasseId,
            })
            .ToListAsync();
        return Ok(perfis);
    }

    [HttpGet("ranking/atributos")]
    public async Task<IActionResult> GetRankingAtributos([FromQuery] int? atributoId, [FromQuery] int top = 50)
    {
        var limite = Math.Clamp(top, 1, 100);

        if (atributoId.HasValue)
        {
            var lista = await db.PontosAtributos
                .AsNoTracking()
                .Where(p => p.AtributoId == atributoId && p.Total > 0 && p.Perfil!.Principal)
                .OrderByDescending(p => p.Total)
                .Take(limite)
                .Select(p => new {
                    p.Perfil!.Id, p.Perfil.Nome, p.Perfil.Nivel, p.Perfil.Rank,
                    p.Perfil.Titulo, p.Perfil.FotoUrl, p.Perfil.ClasseId,
                    TotalPontos = p.Total,
                })
                .ToListAsync();
            return Ok(lista);
        }
        else
        {
            var lista = await (
                from pts in db.PontosAtributos.AsNoTracking()
                where pts.Perfil!.Principal
                group pts by new { pts.PerfilId, pts.Perfil!.Id, pts.Perfil.Nome, pts.Perfil.Nivel, pts.Perfil.Rank, pts.Perfil.Titulo, pts.Perfil.FotoUrl, pts.Perfil.ClasseId }
                into g
                where g.Sum(p => p.Total) > 0
                orderby g.Sum(p => p.Total) descending
                select new {
                    g.Key.Id, g.Key.Nome, g.Key.Nivel, g.Key.Rank,
                    g.Key.Titulo, g.Key.FotoUrl, g.Key.ClasseId,
                    TotalPontos = g.Sum(p => p.Total),
                }
            ).Take(limite).ToListAsync();
            return Ok(lista);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        if (await GarantirDonoDoPerfilAsync(db, id) is { } erro) return erro;
        var perfil = await db.Perfis.FindAsync(id);
        return perfil is null ? NotFound() : Ok(perfil);
    }

    // Retorna perfis sem dono — para o usuário reivindicar os seus após login
    [HttpGet("orfaos")]
    public async Task<IActionResult> GetOrfaos() =>
        Ok(await db.Perfis.Where(p => p.UsuarioId == null).AsNoTracking().ToListAsync());

    // Retorna perfis do usuário autenticado
    [HttpGet("meus")]
    public async Task<IActionResult> GetMeus()
    {
        if (UsuarioId is not int usuarioId) return Unauthorized();

        // Migração: se nenhum perfil é principal, o mais antigo (menor Id) assume
        var temPrincipal = await db.Perfis.AnyAsync(p => p.UsuarioId == usuarioId && p.Principal);
        if (!temPrincipal)
        {
            var maisAntigo = await db.Perfis
                .Where(p => p.UsuarioId == usuarioId)
                .OrderBy(p => p.Id)
                .FirstOrDefaultAsync();
            if (maisAntigo is not null)
            {
                maisAntigo.Principal = true;
                maisAntigo.PrincipalDesde = DateTime.UtcNow;
                await db.SaveChangesAsync();
            }
        }

        var perfis = await db.Perfis
            .Where(p => p.UsuarioId == usuarioId)
            .AsNoTracking()
            .ToListAsync();
        return Ok(perfis);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CriarPerfilDto dto)
    {
        if (UsuarioId is not int usuarioId) return Unauthorized();

        var qtd = await db.Perfis.CountAsync(p => p.UsuarioId == usuarioId);
        if (qtd >= MaxPerfisPorConta)
            return BadRequest($"Limite de {MaxPerfisPorConta} perfis por conta atingido.");

        // Progressão (Xp/Moedas/Nível/Rank) sempre nasce nos defaults do modelo —
        // nunca vem do corpo da requisição, senão dá pra criar herói nível 100 cheio de moedas.
        var perfil = new Perfil
        {
            UsuarioId = usuarioId,
            Nome      = dto.Nome.Trim(),
            ClasseId  = dto.ClasseId,
            Genero    = dto.Genero,
        };
        db.Perfis.Add(perfil);
        await db.SaveChangesAsync();

        // Se é o primeiro perfil da conta, já nasce como principal
        var total = await db.Perfis.CountAsync(p => p.UsuarioId == usuarioId);
        if (total == 1)
        {
            perfil.Principal = true;
            perfil.PrincipalDesde = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        return CreatedAtAction(nameof(GetById), new { id = perfil.Id }, perfil);
    }

    public record CriarPerfilDto(string Nome, int? ClasseId, string? Genero);

    [HttpPost("{id}/definir-principal")]
    public async Task<IActionResult> DefinirPrincipal(int id)
    {
        if (UsuarioId is not int usuarioId) return Unauthorized();
        if (await GarantirDonoDoPerfilAsync(db, id) is { } erro) return erro;

        var perfil = await db.Perfis.FindAsync(id);
        if (perfil is null) return NotFound();

        // Se já é o principal, nada a fazer
        if (perfil.Principal) return Ok(perfil);

        // Cooldown: só pode trocar no dia 1 do mês
        var hoje = DateTime.UtcNow;
        if (hoje.Day != 1)
            return BadRequest($"Só é possível trocar o perfil principal no dia 1 de cada mês. Próxima troca disponível em 01/{hoje.AddMonths(1).Month:D2}/{hoje.AddMonths(1).Year}.");

        // Remove o principal atual
        var principalAtual = await db.Perfis
            .Where(p => p.UsuarioId == usuarioId && p.Principal && p.Id != id)
            .FirstOrDefaultAsync();
        if (principalAtual is not null)
            principalAtual.Principal = false;

        perfil.Principal = true;
        perfil.PrincipalDesde = hoje;
        await db.SaveChangesAsync();
        return Ok(perfil);
    }

    // Remove vínculo de um perfil desta conta (vira órfão novamente)
    [HttpPost("{id}/desvincular")]
    public async Task<IActionResult> Desvincular(int id)
    {
        if (await GarantirDonoDoPerfilAsync(db, id) is { } erro) return erro;

        var perfil = await db.Perfis.FindAsync(id);
        if (perfil is null) return NotFound();

        perfil.UsuarioId = null;
        await db.SaveChangesAsync();
        return Ok();
    }

    // Vincula um perfil convidado (UsuarioId = null) à conta Google autenticada
    [HttpPost("{id}/vincular")]
    public async Task<IActionResult> Vincular(int id)
    {
        if (UsuarioId is not int usuarioId) return Unauthorized();

        var perfil = await db.Perfis.FindAsync(id);
        if (perfil is null) return NotFound();
        if (perfil.UsuarioId is not null) return BadRequest("Perfil já vinculado a uma conta.");

        var qtd = await db.Perfis.CountAsync(p => p.UsuarioId == usuarioId);
        if (qtd >= MaxPerfisPorConta)
            return BadRequest($"Limite de {MaxPerfisPorConta} perfis por conta atingido.");

        perfil.UsuarioId = usuarioId;
        await db.SaveChangesAsync();
        return Ok(perfil);
    }

    [HttpPatch("{id}/info")]
    public async Task<IActionResult> UpdateInfo(int id, UpdatePerfilInfoDto dto)
    {
        if (await GarantirDonoDoPerfilAsync(db, id) is { } erro) return erro;

        var perfil = await db.Perfis.FindAsync(id);
        if (perfil is null) return NotFound();
        if (!string.IsNullOrWhiteSpace(dto.Nome))
            perfil.Nome = dto.Nome.Trim();
        perfil.ClasseId = dto.ClasseId;
        perfil.Genero   = dto.Genero;
        await db.SaveChangesAsync();
        return Ok(perfil);
    }

    public record UpdatePerfilInfoDto(string Nome, int? ClasseId, string? Genero);

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (await GarantirDonoDoPerfilAsync(db, id) is { } erro) return erro;

        var perfil = await db.Perfis.FindAsync(id);
        if (perfil is null) return NotFound();
        db.Perfis.Remove(perfil);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/xp")]
    public async Task<IActionResult> AdicionarXp(int id, [FromQuery] int quantidade)
    {
        if (await GarantirDonoDoPerfilAsync(db, id) is { } erro) return erro;
        await xpService.AdicionarXpAsync(id, quantidade);
        return Ok(await db.Perfis.FindAsync(id));
    }

    [HttpPost("{id}/reset")]
    public async Task<IActionResult> Reset(int id)
    {
        if (await GarantirDonoDoPerfilAsync(db, id) is { } erro) return erro;

        var perfil = await db.Perfis.FindAsync(id);
        if (perfil is null) return NotFound();

        perfil.Xp = 0;
        perfil.Moedas = 0;
        perfil.Nivel = 1;
        perfil.ProximoNivelXp = 100;
        perfil.Rank = "H";
        perfil.Titulo = "Iniciante";

        // Remove as ativações do perfil (as definições do catálogo permanecem)
        db.PerfilBonsHabitos.RemoveRange(db.PerfilBonsHabitos.Where(a => a.PerfilId == id));
        db.PerfilMausHabitos.RemoveRange(db.PerfilMausHabitos.Where(a => a.PerfilId == id));
        db.PerfilMissoes.RemoveRange(db.PerfilMissoes.Where(a => a.PerfilId == id));

        await db.SaveChangesAsync();
        return Ok(perfil);
    }

    [HttpPost("{id}/foto")]
    public async Task<IActionResult> UploadFoto(int id, IFormFile arquivo)
    {
        if (await GarantirDonoDoPerfilAsync(db, id) is { } erro) return erro;

        var perfil = await db.Perfis.FindAsync(id);
        if (perfil is null) return NotFound();

        var mimePermitidos = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
        var mime = arquivo.ContentType.ToLowerInvariant();
        if (!mimePermitidos.Contains(mime))
            return BadRequest("Formato inválido. Use JPG, PNG, GIF ou WebP.");

        if (arquivo.Length > Limites.TamanhoMaxFotoBytes)
            return BadRequest("Arquivo muito grande. Máximo 5MB.");

        using var ms = new MemoryStream();
        await arquivo.CopyToAsync(ms);
        var base64 = Convert.ToBase64String(ms.ToArray());
        perfil.FotoUrl = $"data:{mime};base64,{base64}";

        await db.SaveChangesAsync();
        return Ok(perfil);
    }

    // ── Desafio do dia ────────────────────────────────────────────────

    [HttpPost("{id}/desafio/recusar")]
    public async Task<IActionResult> RecusarDesafio(int id)
    {
        if (await GarantirDonoDoPerfilAsync(db, id) is { } erro) return erro;

        var perfil = await db.Perfis.FindAsync(id);
        if (perfil is null) return NotFound();
        perfil.DesafioRecusadoEm = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(perfil);
    }

    [HttpPost("{id}/desafio/concluir")]
    public async Task<IActionResult> ConcluirDesafio(int id)
    {
        if (await GarantirDonoDoPerfilAsync(db, id) is { } erro) return erro;

        var perfil = await db.Perfis.FindAsync(id);
        if (perfil is null) return NotFound();
        perfil.DesafioConcluidoEm = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(perfil);
    }

    // ── Lootbox ──────────────────────────────────────────────────────

    [HttpGet("{id}/lootbox/status")]
    public async Task<IActionResult> LootboxStatus(int id)
    {
        if (await GarantirDonoDoPerfilAsync(db, id) is { } erro) return erro;
        var status = await lootbox.StatusAsync(id);
        return status is null ? NotFound() : Ok(status);
    }

    [HttpPost("{id}/lootbox/abrir")]
    public async Task<IActionResult> AbrirLootbox(int id)
    {
        if (await GarantirDonoDoPerfilAsync(db, id) is { } erro) return erro;
        var resultado = await lootbox.AbrirAsync(id);
        return resultado.Sucesso
            ? Ok(new { recompensa = resultado.Recompensa, chance = resultado.Chance })
            : BadRequest(resultado.Erro);
    }
}
