using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IsekaiDesnecessario.API.Data;
using IsekaiDesnecessario.API.Models;
using System.Linq;

namespace IsekaiDesnecessario.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecompensasController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int perfilId) =>
        Ok(await db.Recompensas
            .Where(r => r.PerfilId == perfilId)
            .OrderBy(r => r.Preco)
            .ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create(Recompensa recompensa)
    {
        db.Recompensas.Add(recompensa);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), recompensa);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Recompensa recompensa)
    {
        var r = await db.Recompensas.FindAsync(id);
        if (r is null) return NotFound();
        r.Nome              = recompensa.Nome;
        r.Descricao         = recompensa.Descricao;
        r.Emoji             = recompensa.Emoji;
        r.Preco             = recompensa.Preco;
        r.Ativa             = recompensa.Ativa;
        r.AtributoId        = recompensa.AtributoId;
        r.PontosNecessarios = recompensa.PontosNecessarios;
        await db.SaveChangesAsync();
        return Ok(r);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var r = await db.Recompensas.FindAsync(id);
        if (r is null) return NotFound();
        db.Recompensas.Remove(r);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/resgatar")]
    public async Task<IActionResult> Resgatar(int id, [FromQuery] int perfilId)
    {
        var recompensa = await db.Recompensas.FindAsync(id);
        if (recompensa is null) return NotFound();

        var perfil = await db.Perfis.FindAsync(perfilId);
        if (perfil is null) return NotFound("Perfil não encontrado.");

        if (perfil.Moedas < recompensa.Preco)
            return BadRequest("Moedas insuficientes.");

        // Valida requisito de atributo
        if (recompensa.AtributoId.HasValue && recompensa.PontosNecessarios > 0)
        {
            var bons = await db.BonsHabitos
                .Where(h => h.PerfilId == perfilId && h.AtributoId == recompensa.AtributoId)
                .ToListAsync();
            var missoes = await db.Missoes
                .Where(m => m.PerfilId == perfilId && m.AtributoId == recompensa.AtributoId && m.Concluida)
                .ToListAsync();

            int pontosAtributo = (bons.Sum(h => h.Xp * h.Streak) + missoes.Sum(m => m.RecompensaXp)) / 10;

            if (pontosAtributo < recompensa.PontosNecessarios)
                return BadRequest($"Atributo insuficiente. Você tem {pontosAtributo} pts, precisa de {recompensa.PontosNecessarios}.");
        }

        perfil.Moedas -= recompensa.Preco;

        db.DiarioAcoes.Add(new() { PerfilId = perfilId, Emoji = recompensa.Emoji, Tipo = "recompensa",
            Mensagem = $"Resgatou \"{recompensa.Nome}\" por {recompensa.Preco} moedas" });

        // Adiciona ao inventário
        db.Inventario.Add(new IsekaiDesnecessario.API.Models.ItemInventario
        {
            PerfilId    = perfilId,
            RecompensaId = recompensa.Id,
            Nome        = recompensa.Nome,
            Emoji       = recompensa.Emoji,
            Descricao   = recompensa.Descricao,
            Preco       = recompensa.Preco,
            DataCompra  = DateTime.Now,
        });

        await db.SaveChangesAsync();
        return Ok(perfil);
    }
}
