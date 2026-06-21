using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IsekaiDesnecessario.API.Data;
using IsekaiDesnecessario.API.Models;

namespace IsekaiDesnecessario.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RecompensasController(AppDbContext db) : ApiControllerBase
{
    // Recompensas ativas na loja do perfil (definição achatada)
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int perfilId)
    {
        if (await GarantirDonoDoPerfilAsync(db, perfilId) is { } erro) return erro;
        var lista = await db.PerfilRecompensas
            .Where(a => a.PerfilId == perfilId && a.Ativo)
            .OrderBy(a => a.Recompensa!.Preco)
            .Select(a => new RecompensaDto(a.Recompensa!.Id, perfilId, a.Recompensa.Nome, a.Recompensa.Descricao,
                a.Recompensa.Emoji, a.Recompensa.Preco, a.Recompensa.Ativa, a.Recompensa.AtributoId, a.Recompensa.PontosNecessarios))
            .AsNoTracking().ToListAsync();
        return Ok(lista);
    }

    [HttpGet("catalogo")]
    public async Task<IActionResult> Catalogo([FromQuery] int perfilId)
    {
        if (await GarantirDonoDoPerfilAsync(db, perfilId) is { } erro) return erro;
        var uid = UsuarioId;
        var classeId = await ClasseDoPerfilAsync(db, perfilId);
        var lista = await db.Recompensas
            .Where(d => d.Status == StatusConteudo.Aprovado
                     && (d.Escopo == EscopoConteudo.Global || d.CriadoPorUsuarioId == uid))
            .Select(d => new RecompensaCatalogoDto(d.Id, d.Nome, d.Descricao, d.Emoji, d.Preco, d.Ativa,
                d.AtributoId, d.PontosNecessarios, d.Escopo, d.Status,
                db.PerfilRecompensas.Any(a => a.PerfilId == perfilId && a.RecompensaId == d.Id && a.Ativo),
                d.Classes.Select(c => c.Id).ToArray(),
                d.Classes.Any() && !d.Classes.Any(c => c.Id == classeId)))
            .AsNoTracking().ToListAsync();
        return Ok(lista);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CriarRecompensaDto dto)
    {
        if (await GarantirDonoDoPerfilAsync(db, dto.PerfilId) is { } erro) return erro;
        var autoria = DefinirAutoria(await ObterRoleAsync(db), dto.Proprio);
        if (autoria is null)
            return StatusCode(StatusCodes.Status403Forbidden, "Seu papel não pode criar esse conteúdo.");
        var (escopo, status) = autoria.Value;

        var def = new Recompensa
        {
            Nome = dto.Nome, Descricao = dto.Descricao, Emoji = string.IsNullOrWhiteSpace(dto.Emoji) ? "🎁" : dto.Emoji,
            Preco = dto.Preco, AtributoId = dto.AtributoId, PontosNecessarios = dto.PontosNecessarios,
            TravaDias = dto.TravaDias, Escopo = escopo, Status = status, CriadoPorUsuarioId = UsuarioId,
        };
        if (escopo == EscopoConteudo.Global && dto.ClasseIds is { Count: > 0 })
            def.Classes = await db.Classes.Where(c => dto.ClasseIds.Contains(c.Id)).ToListAsync();

        var classeId = await ClasseDoPerfilAsync(db, dto.PerfilId);
        if (def.Classes.Count == 0 || (classeId != null && def.Classes.Any(c => c.Id == classeId)))
            def.Ativacoes.Add(new PerfilRecompensa { PerfilId = dto.PerfilId, Ativo = true });
        db.Recompensas.Add(def);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { perfilId = dto.PerfilId },
            new RecompensaDto(def.Id, dto.PerfilId, def.Nome, def.Descricao, def.Emoji, def.Preco, def.Ativa, def.AtributoId, def.PontosNecessarios));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Recompensa recompensa)
    {
        var d = await db.Recompensas.FindAsync(id);
        if (d is null) return NotFound();
        if (!await PodeMutarDefinicaoAsync(db, d.CriadoPorUsuarioId, d.Escopo, d.Status)) return Forbid();
        d.Nome              = recompensa.Nome;
        d.Descricao         = recompensa.Descricao;
        d.Emoji             = recompensa.Emoji;
        d.Preco             = recompensa.Preco;
        d.Ativa             = recompensa.Ativa;
        d.AtributoId        = recompensa.AtributoId;
        d.PontosNecessarios = recompensa.PontosNecessarios;
        await db.SaveChangesAsync();
        return Ok(d);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var d = await db.Recompensas.FindAsync(id);
        if (d is null) return NotFound();
        if (!await PodeMutarDefinicaoAsync(db, d.CriadoPorUsuarioId, d.Escopo, d.Status)) return Forbid();
        db.Recompensas.Remove(d); // ativações e nada mais (inventário é desnormalizado)
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/ativar")]
    public async Task<IActionResult> Ativar(int id, [FromQuery] int perfilId) =>
        await AtivarAsync(id, perfilId, ativar: true);

    [HttpPost("{id}/desativar")]
    public async Task<IActionResult> Desativar(int id, [FromQuery] int perfilId) =>
        await AtivarAsync(id, perfilId, ativar: false);

    [HttpPost("{id}/resgatar")]
    public async Task<IActionResult> Resgatar(int id, [FromQuery] int perfilId)
    {
        if (await GarantirDonoDoPerfilAsync(db, perfilId) is { } erro) return erro;

        var ativacao = await db.PerfilRecompensas.Include(a => a.Recompensa)
            .FirstOrDefaultAsync(a => a.PerfilId == perfilId && a.RecompensaId == id && a.Ativo);
        if (ativacao?.Recompensa is null) return NotFound();
        var recompensa = ativacao.Recompensa;

        var perfil = await db.Perfis.FindAsync(perfilId);
        if (perfil is null) return NotFound("Perfil não encontrado.");
        if (perfil.Moedas < recompensa.Preco)
            return BadRequest("Moedas insuficientes.");

        // Valida requisito de atributo (soma do estado por-perfil das ativações)
        if (recompensa.AtributoId.HasValue && recompensa.PontosNecessarios > 0)
        {
            var xpBons = await db.PerfilBonsHabitos
                .Where(a => a.PerfilId == perfilId && a.Ativo && a.BomHabito!.AtributoId == recompensa.AtributoId)
                .SumAsync(a => a.BomHabito!.Xp * a.Streak);
            var xpMiss = await db.PerfilMissoes
                .Where(a => a.PerfilId == perfilId && a.Ativo && a.Concluida && a.Missao!.AtributoId == recompensa.AtributoId)
                .SumAsync(a => a.Missao!.RecompensaXp);

            int pontosAtributo = (xpBons + xpMiss) / 10;
            if (pontosAtributo < recompensa.PontosNecessarios)
                return BadRequest($"Atributo insuficiente. Você tem {pontosAtributo} pts, precisa de {recompensa.PontosNecessarios}.");
        }

        perfil.Moedas -= recompensa.Preco;
        if (recompensa.TravaDias > 0) ativacao.TravadoAte = DateTime.UtcNow.AddDays(recompensa.TravaDias);

        db.DiarioAcoes.Add(new() { PerfilId = perfilId, Emoji = recompensa.Emoji, Tipo = "recompensa",
            Mensagem = $"Resgatou \"{recompensa.Nome}\" por {recompensa.Preco} moedas" });

        db.Inventario.Add(new ItemInventario
        {
            PerfilId     = perfilId,
            RecompensaId = recompensa.Id,
            Nome         = recompensa.Nome,
            Emoji        = recompensa.Emoji,
            Descricao    = recompensa.Descricao,
            Preco        = recompensa.Preco,
            DataCompra   = DateTime.UtcNow,
        });

        await db.SaveChangesAsync();
        return Ok(perfil);
    }

    private async Task<IActionResult> AtivarAsync(int defId, int perfilId, bool ativar)
    {
        if (await GarantirDonoDoPerfilAsync(db, perfilId) is { } erro) return erro;
        var uid = UsuarioId;
        var info = await db.Recompensas
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

        var a = await db.PerfilRecompensas.FirstOrDefaultAsync(x => x.PerfilId == perfilId && x.RecompensaId == defId);
        if (!ativar && Travado(a?.TravadoAte) is { } travaErro) return travaErro;
        if (a is null) db.PerfilRecompensas.Add(new() { PerfilId = perfilId, RecompensaId = defId, Ativo = ativar });
        else a.Ativo = ativar;
        await db.SaveChangesAsync();
        return Ok();
    }
}

// ClasseIds: vínculo multi-classe (só aplicado a conteúdo global).
// Proprio: true = conteúdo privado da conta (VIP/Mod/Admin); false = catálogo global.
public record CriarRecompensaDto(int PerfilId, string Nome, string Descricao, string Emoji,
    int Preco, int? AtributoId, int PontosNecessarios, List<int>? ClasseIds, bool Proprio, int TravaDias);

// Recompensa ativa na loja do perfil (definição achatada).
public record RecompensaDto(int Id, int PerfilId, string Nome, string Descricao, string Emoji,
    int Preco, bool Ativa, int? AtributoId, int PontosNecessarios);

// Recompensa do catálogo (definição + se já está ativa no perfil + classes/bloqueio).
public record RecompensaCatalogoDto(int Id, string Nome, string Descricao, string Emoji,
    int Preco, bool Ativa, int? AtributoId, int PontosNecessarios,
    EscopoConteudo Escopo, StatusConteudo Status, bool Ativo, int[] ClasseIds, bool Bloqueado);
