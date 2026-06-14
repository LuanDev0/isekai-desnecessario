using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IsekaiDesnecessario.API.Data;

namespace IsekaiDesnecessario.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiarioAcoesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetRecentes([FromQuery] int perfilId, [FromQuery] int limit = 30) =>
        Ok(await db.DiarioAcoes
            .Where(d => d.PerfilId == perfilId)
            .OrderByDescending(d => d.Data)
            .Take(limit)
            .ToListAsync());
}
