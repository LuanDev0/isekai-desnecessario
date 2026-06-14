using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IsekaiDesnecessario.API.Data;
using IsekaiDesnecessario.API.Models;

namespace IsekaiDesnecessario.API.Controllers;

[ApiController]
[Route("api/snapshots")]
public class SnapshotsController(AppDbContext db) : ControllerBase
{
    // GET /api/snapshots/anterior?perfilId=1
    // Retorna o snapshot mais recente (antes de hoje) para cada atributo
    [HttpGet("anterior")]
    public async Task<IActionResult> GetAnterior([FromQuery] int perfilId)
    {
        var hoje = DateTime.Today;

        // Pega o dia mais recente que tenha snapshot antes de hoje
        var diasDisponiveis = await db.SnapshotsAtributo
            .Where(s => s.PerfilId == perfilId && s.Data < hoje)
            .Select(s => s.Data)
            .Distinct()
            .OrderByDescending(d => d)
            .Take(1)
            .ToListAsync();

        if (!diasDisponiveis.Any())
            return Ok(new List<object>());

        var diaAnterior = diasDisponiveis[0];

        var snapshots = await db.SnapshotsAtributo
            .Where(s => s.PerfilId == perfilId && s.Data == diaAnterior)
            .Select(s => new { s.AtributoId, s.Pontos, s.Data })
            .ToListAsync();

        return Ok(snapshots);
    }

    // POST /api/snapshots/salvar?perfilId=1
    // Salva snapshot dos atributos atuais (idempotente — uma vez por dia)
    [HttpPost("salvar")]
    public async Task<IActionResult> Salvar([FromQuery] int perfilId, [FromBody] List<SnapshotDto> dados)
    {
        var hoje = DateTime.Today;

        // Verifica se já salvou hoje
        var jaExiste = await db.SnapshotsAtributo
            .AnyAsync(s => s.PerfilId == perfilId && s.Data == hoje);

        if (jaExiste)
            return Ok(new { message = "Snapshot já existe para hoje" });

        var novos = dados.Select(d => new SnapshotAtributo
        {
            PerfilId   = perfilId,
            AtributoId = d.AtributoId,
            Pontos     = d.Pontos,
            Data       = hoje,
        }).ToList();

        db.SnapshotsAtributo.AddRange(novos);
        await db.SaveChangesAsync();
        return Ok(new { message = "Snapshot salvo", total = novos.Count });
    }
}

public record SnapshotDto(int AtributoId, int Pontos);
