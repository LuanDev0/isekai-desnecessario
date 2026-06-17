using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IsekaiDesnecessario.API.Data;
using IsekaiDesnecessario.API.Models;
using IsekaiDesnecessario.API.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace IsekaiDesnecessario.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PerfilController(AppDbContext db, XpService xpService) : ControllerBase
{
    // Máximo de heróis (perfis) que uma conta Google pode ter
    public const int MaxPerfisPorConta = 3;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await db.Perfis.ToListAsync());

    [HttpGet("me")]
    public async Task<IActionResult> GetOrCreate()
    {
        var perfil = await db.Perfis.FirstOrDefaultAsync();

        if (perfil is null)
        {
            perfil = new Perfil
            {
                Nome = "Jogador",
                Titulo = "Iniciante",
                Rank = "H",
                Nivel = 1,
                Xp = 0,
                ProximoNivelXp = 100,
                Moedas = 0,
            };
            db.Perfis.Add(perfil);
            await db.SaveChangesAsync();
        }

        return Ok(perfil);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var perfil = await db.Perfis.FindAsync(id);
        return perfil is null ? NotFound() : Ok(perfil);
    }

    // Retorna perfis sem dono (UsuarioId = null) — para recuperação após login
    [HttpGet("orfaos")]
    [Authorize]
    public async Task<IActionResult> GetOrfaos()
    {
        var perfis = await db.Perfis.Where(p => p.UsuarioId == null).ToListAsync();
        return Ok(perfis);
    }

    // Retorna perfis do usuário autenticado
    [HttpGet("meus")]
    [Authorize]
    public async Task<IActionResult> GetMeus()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (!int.TryParse(sub, out int usuarioId)) return Unauthorized();
        var perfis = await db.Perfis.Where(p => p.UsuarioId == usuarioId).ToListAsync();
        return Ok(perfis);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Perfil perfil)
    {
        // Se há JWT válido, associa o perfil ao usuário autenticado
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (int.TryParse(sub, out int usuarioIdJwt))
            perfil.UsuarioId = usuarioIdJwt;

        // Limite de perfis por conta Google (convidados, com UsuarioId null, não contam)
        if (perfil.UsuarioId is int usuarioId)
        {
            var qtd = await db.Perfis.CountAsync(p => p.UsuarioId == usuarioId);
            if (qtd >= MaxPerfisPorConta)
                return BadRequest($"Limite de {MaxPerfisPorConta} perfis por conta atingido.");
        }

        db.Perfis.Add(perfil);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = perfil.Id }, perfil);
    }

    // Vincula um perfil convidado (UsuarioId = null) à conta Google autenticada
    [HttpPost("{id}/vincular")]
    [Authorize]
    public async Task<IActionResult> Vincular(int id)
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (!int.TryParse(sub, out int usuarioId)) return Unauthorized();

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

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Perfil perfil)
    {
        if (id != perfil.Id) return BadRequest();
        db.Entry(perfil).State = EntityState.Modified;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id}/info")]
    public async Task<IActionResult> UpdateInfo(int id, UpdatePerfilInfoDto dto)
    {
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
        var perfil = await db.Perfis.FindAsync(id);
        if (perfil is null) return NotFound();
        db.Perfis.Remove(perfil);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/xp")]
    public async Task<IActionResult> AdicionarXp(int id, [FromQuery] int quantidade)
    {
        await xpService.AdicionarXp(id, quantidade);
        return Ok(await db.Perfis.FindAsync(id));
    }

    [HttpPost("{id}/reset")]
    public async Task<IActionResult> Reset(int id)
    {
        var perfil = await db.Perfis.FindAsync(id);
        if (perfil is null) return NotFound();

        perfil.Xp = 0;
        perfil.Moedas = 0;
        perfil.Nivel = 1;
        perfil.ProximoNivelXp = 100;
        perfil.Rank = "H";
        perfil.Titulo = "Iniciante";

        db.BonsHabitos.RemoveRange(db.BonsHabitos.Where(h => h.PerfilId == id));
        db.MausHabitos.RemoveRange(db.MausHabitos.Where(h => h.PerfilId == id));
        db.Missoes.RemoveRange(db.Missoes.Where(m => m.PerfilId == id));

        await db.SaveChangesAsync();
        return Ok(perfil);
    }

    [HttpPost("{id}/foto")]
    public async Task<IActionResult> UploadFoto(int id, IFormFile arquivo)
    {
        var perfil = await db.Perfis.FindAsync(id);
        if (perfil is null) return NotFound();

        var mimePermitidos = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
        var mime = arquivo.ContentType.ToLowerInvariant();
        if (!mimePermitidos.Contains(mime))
            return BadRequest("Formato inválido. Use JPG, PNG, GIF ou WebP.");

        if (arquivo.Length > 5 * 1024 * 1024)
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
        var perfil = await db.Perfis.FindAsync(id);
        if (perfil is null) return NotFound();
        perfil.DesafioRecusadoEm = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(perfil);
    }

    [HttpPost("{id}/desafio/concluir")]
    public async Task<IActionResult> ConcluirDesafio(int id)
    {
        var perfil = await db.Perfis.FindAsync(id);
        if (perfil is null) return NotFound();
        perfil.DesafioConcluídoEm = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(perfil);
    }

    // ── Lootbox ──────────────────────────────────────────────────────

    [HttpGet("{id}/lootbox/status")]
    public async Task<IActionResult> LootboxStatus(int id)
    {
        var perfil = await db.Perfis.FindAsync(id);
        if (perfil is null) return NotFound();

        // Reseta XpHoje se for um dia novo
        if (perfil.DataXpHoje?.Date != DateTime.UtcNow.Date)
        {
            perfil.XpHoje     = 0;
            perfil.DataXpHoje = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        bool xpSuficiente  = perfil.XpHoje >= 1000;
        bool naoPegouHoje  = perfil.UltimaLootbox?.Date != DateTime.UtcNow.Date;
        bool disponivel    = xpSuficiente && naoPegouHoje;

        return Ok(new
        {
            disponivel,
            xpHoje      = perfil.XpHoje,
            xpNecessario = 1000,
            jaAbriuHoje  = !naoPegouHoje
        });
    }

    [HttpPost("{id}/lootbox/abrir")]
    public async Task<IActionResult> AbrirLootbox(int id)
    {
        var perfil = await db.Perfis.FindAsync(id);
        if (perfil is null) return NotFound();

        if (perfil.DataXpHoje?.Date != DateTime.UtcNow.Date)
        { perfil.XpHoje = 0; perfil.DataXpHoje = DateTime.UtcNow; }

        if (perfil.XpHoje < 1000)
            return BadRequest("XP insuficiente. Ganhe 1000 XP hoje para abrir a lootbox.");

        if (perfil.UltimaLootbox?.Date == DateTime.UtcNow.Date)
            return BadRequest("Lootbox já aberta hoje. Volte amanhã!");

        var recompensas = await db.Recompensas
            .Where(r => r.PerfilId == id && r.Ativa)
            .ToListAsync();

        if (recompensas.Count == 0)
            return BadRequest("Nenhuma recompensa cadastrada.");

        // Seleção ponderada: peso = precoMaximo - preco + 1 (mais barato = mais chance)
        int precoMax   = recompensas.Max(r => r.Preco);
        var pesos      = recompensas.Select(r => new { r, peso = precoMax - r.Preco + 1 }).ToList();
        int totalPeso  = pesos.Sum(p => p.peso);
        int roll       = Random.Shared.Next(totalPeso);
        int acumulado  = 0;
        Recompensa? ganhador = null;

        foreach (var p in pesos)
        {
            acumulado += p.peso;
            if (roll < acumulado) { ganhador = p.r; break; }
        }

        perfil.UltimaLootbox = DateTime.UtcNow;

        db.DiarioAcoes.Add(new IsekaiDesnecessario.API.Models.DiarioAcao { PerfilId = id, Emoji = "📦", Tipo = "lootbox",
            Mensagem = $"Abriu lootbox e ganhou \"{ganhador!.Nome}\"" });

        await db.SaveChangesAsync();

        int pesoGanhador  = precoMax - ganhador!.Preco + 1;
        double chance     = Math.Round((double)pesoGanhador / totalPeso * 100, 1);

        return Ok(new { recompensa = ganhador, chance });
    }
}
