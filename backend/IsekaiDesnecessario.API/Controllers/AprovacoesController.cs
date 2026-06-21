using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IsekaiDesnecessario.API.Data;
using IsekaiDesnecessario.API.Models;
using IsekaiDesnecessario.API.Services;

namespace IsekaiDesnecessario.API.Controllers;

// Fluxo de aprovação Moderador → Admin. Só Admin acessa.
[ApiController]
[Route("api/aprovacoes")]
[Authorize]
public class AprovacoesController(AppDbContext db, NotificacaoService notificacoes) : ApiControllerBase
{
    // Itens globais pendentes de aprovação, unificados por tipo.
    [HttpGet("pendentes")]
    public async Task<IActionResult> Pendentes()
    {
        if (await ObterRoleAsync(db) != Role.Admin) return Forbid();

        var bons = await db.BonsHabitos
            .Where(d => d.Status == StatusConteudo.Pendente && d.Escopo == EscopoConteudo.Global)
            .Select(d => new PendenteDto("bomhabito", d.Id, d.Habito, d.CriadoPorUsuarioId, d.CriadoPor!.Nome))
            .AsNoTracking().ToListAsync();
        var maus = await db.MausHabitos
            .Where(d => d.Status == StatusConteudo.Pendente && d.Escopo == EscopoConteudo.Global)
            .Select(d => new PendenteDto("mauhabito", d.Id, d.Habito, d.CriadoPorUsuarioId, d.CriadoPor!.Nome))
            .AsNoTracking().ToListAsync();
        var miss = await db.Missoes
            .Where(d => d.Status == StatusConteudo.Pendente && d.Escopo == EscopoConteudo.Global)
            .Select(d => new PendenteDto("missao", d.Id, d.Titulo, d.CriadoPorUsuarioId, d.CriadoPor!.Nome))
            .AsNoTracking().ToListAsync();
        var recs = await db.Recompensas
            .Where(d => d.Status == StatusConteudo.Pendente && d.Escopo == EscopoConteudo.Global)
            .Select(d => new PendenteDto("recompensa", d.Id, d.Nome, d.CriadoPorUsuarioId, d.CriadoPor!.Nome))
            .AsNoTracking().ToListAsync();

        return Ok(bons.Concat(maus).Concat(miss).Concat(recs).ToList());
    }

    [HttpPost("{tipo}/{id}/aprovar")]
    public async Task<IActionResult> Aprovar(string tipo, int id) =>
        await DecidirAsync(tipo, id, StatusConteudo.Aprovado);

    [HttpPost("{tipo}/{id}/rejeitar")]
    public async Task<IActionResult> Rejeitar(string tipo, int id) =>
        await DecidirAsync(tipo, id, StatusConteudo.Rejeitado);

    private async Task<IActionResult> DecidirAsync(string tipo, int id, StatusConteudo novo)
    {
        if (await ObterRoleAsync(db) != Role.Admin) return Forbid();

        IDefinicaoConteudo? d = tipo.ToLowerInvariant() switch
        {
            "bomhabito"  => await db.BonsHabitos.FindAsync(id),
            "mauhabito"  => await db.MausHabitos.FindAsync(id),
            "missao"     => await db.Missoes.FindAsync(id),
            "recompensa" => await db.Recompensas.FindAsync(id),
            _            => null,
        };
        if (d is null) return NotFound();
        if (d.Escopo != EscopoConteudo.Global || d.Status != StatusConteudo.Pendente)
            return BadRequest("Item não está pendente de aprovação.");

        d.Status = novo;
        await db.SaveChangesAsync();

        // Avisa o autor (moderador) quando rejeitado
        if (novo == StatusConteudo.Rejeitado && d.CriadoPorUsuarioId is int autor)
            await notificacoes.NotificarAsync(autor, "rejeitado", $"Seu item \"{d.Titulo}\" foi rejeitado pelo admin.");
        return Ok();
    }
}

// Item pendente de aprovação (unificado entre tipos). Titulo = Habito/Titulo/Nome.
public record PendenteDto(string Tipo, int Id, string Titulo, int? CriadoPorUsuarioId, string? AutorNome);
