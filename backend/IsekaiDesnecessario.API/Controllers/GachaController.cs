using Microsoft.AspNetCore.Mvc;
using IsekaiDesnecessario.API.Services;

namespace IsekaiDesnecessario.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GachaController(GachaService gachaService) : ControllerBase
{
    [HttpPost("sortear/{perfilId}")]
    public async Task<IActionResult> Sortear(int perfilId)
    {
        var recompensa = await gachaService.Sortear(perfilId);
        if (recompensa is null) return NotFound("Nenhuma recompensa cadastrada.");
        return Ok(recompensa);
    }
}
