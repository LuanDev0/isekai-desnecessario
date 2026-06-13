using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoloLeveling.API.Data;

namespace SoloLeveling.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AtributosController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await db.Atributos.OrderBy(a => a.Id).ToListAsync());
}
