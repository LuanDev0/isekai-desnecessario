using Google.Apis.Auth;
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
        catch (InvalidJwtException)
        {
            // Token Google malformado/expirado/assinatura inválida.
            // Não expõe detalhes internos ao cliente.
            return Unauthorized(new { erro = "Token Google inválido." });
        }
    }

    [HttpPost("registrar")]
    public async Task<IActionResult> Registrar([FromBody] RegistrarDto dto)
    {
        try
        {
            var (jwt, usuario, perfis) = await authService.Registrar(dto.Nome, dto.Email, dto.Senha);
            return Ok(new { token = jwt, usuario, perfis });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { erro = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        try
        {
            var (jwt, usuario, perfis) = await authService.Login(dto.Email, dto.Senha);
            return Ok(new { token = jwt, usuario, perfis });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { erro = ex.Message });
        }
    }
}

public record GoogleLoginDto(string IdToken);
public record RegistrarDto(string Nome, string Email, string Senha);
public record LoginDto(string Email, string Senha);
