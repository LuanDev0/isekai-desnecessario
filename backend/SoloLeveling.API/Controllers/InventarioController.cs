using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoloLeveling.API.Data;

namespace SoloLeveling.API.Controllers;

[ApiController]
[Route("api/inventario")]
public class InventarioController(AppDbContext db) : ControllerBase
{
    // GET api/inventario?perfilId=X
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int perfilId)
    {
        var itens = await db.Inventario
            .Where(i => i.PerfilId == perfilId)
            .OrderByDescending(i => i.DataCompra)
            .ToListAsync();
        return Ok(itens);
    }

    // POST api/inventario/{id}/usar
    [HttpPost("{id}/usar")]
    public async Task<IActionResult> Usar(int id, [FromQuery] int perfilId)
    {
        var item = await db.Inventario.FirstOrDefaultAsync(i => i.Id == id && i.PerfilId == perfilId);
        if (item is null) return NotFound();
        if (item.Usado) return BadRequest("Item já foi usado.");

        item.Usado   = true;
        item.DataUso = DateTime.Now;
        await db.SaveChangesAsync();
        return Ok(item);
    }
}
