using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IsekaiDesnecessario.API.Data;

namespace IsekaiDesnecessario.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DiarioAcoesController(AppDbContext db) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetRecentes([FromQuery] int perfilId, [FromQuery] int limit = 30)
    {
        if (await GarantirDonoDoPerfil(db, perfilId) is { } erro) return erro;
        return Ok(await db.DiarioAcoes
            .Where(d => d.PerfilId == perfilId)
            .OrderByDescending(d => d.Data)
            .Take(limit)
            .AsNoTracking()
            .ToListAsync());
    }
}
