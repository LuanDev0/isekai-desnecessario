using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IsekaiDesnecessario.API.Data;

namespace IsekaiDesnecessario.API.Controllers;

[ApiController]
[Route("api/notificacoes")]
[Authorize]
public class NotificacoesController(AppDbContext db) : ApiControllerBase
{
    // Notificações da conta logada (mais recentes primeiro).
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        if (UsuarioId is not int uid) return Unauthorized();
        var lista = await db.Notificacoes
            .Where(n => n.UsuarioId == uid)
            .OrderByDescending(n => n.CriadaEm)
            .Take(50)
            .AsNoTracking().ToListAsync();
        return Ok(lista);
    }

    [HttpPost("{id}/lida")]
    public async Task<IActionResult> MarcarLida(int id)
    {
        if (UsuarioId is not int uid) return Unauthorized();
        var n = await db.Notificacoes.FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == uid);
        if (n is null) return NotFound();
        n.Lida = true;
        await db.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("lidas")]
    public async Task<IActionResult> MarcarTodasLidas()
    {
        if (UsuarioId is not int uid) return Unauthorized();
        await db.Notificacoes
            .Where(n => n.UsuarioId == uid && !n.Lida)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.Lida, true));
        return Ok();
    }
}
