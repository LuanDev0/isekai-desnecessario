using Microsoft.AspNetCore.Mvc;
using IsekaiDesnecessario.API.Services;

namespace IsekaiDesnecessario.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AuthService authService) : ControllerBase
{
    [HttpPost("google")]
    public async Task<IActionResult> Google([FromBody] GoogleLoginDto dto)
    {
        try
        {
            var (jwt, usuario, perfis) = await authService.LoginComGoogle(dto.IdToken);
            return Ok(new { token = jwt, usuario, perfis });
        }
        catch (Exception ex)
        {
            return Unauthorized(new { erro = "Token Google inválido.", detalhe = ex.Message });
        }
    }
}

public record GoogleLoginDto(string IdToken);
