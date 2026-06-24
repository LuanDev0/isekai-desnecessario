using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IsekaiDesnecessario.API.Data;

namespace IsekaiDesnecessario.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PontosAtributosController(AppDbContext db) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int perfilId)
    {
        if (await GarantirDonoDoPerfilAsync(db, perfilId) is { } erro) return erro;
        var lista = await db.PontosAtributos
            .Where(p => p.PerfilId == perfilId)
            .Select(p => new { p.AtributoId, p.Total })
            .ToListAsync();
        return Ok(lista);
    }
}
