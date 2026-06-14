using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IsekaiDesnecessario.API.Data;

namespace IsekaiDesnecessario.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClassesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await db.Classes
            .Include(c => c.Atributo)
            .Select(c => new {
                c.Id,
                c.Nome,
                c.NomeFeminino,
                c.Emoji,
                c.AtributoId,
                atributo = c.Atributo == null ? null : new {
                    c.Atributo.Id,
                    c.Atributo.Nome,
                    c.Atributo.Emoji,
                    c.Atributo.Descricao,
                    c.Atributo.Cor
                }
            })
            .ToListAsync());
}
