using IsekaiDesnecessario.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace IsekaiDesnecessario.API.Controllers;

// Base com helpers de autorização por perfil.
// Todo endpoint que mexe em dados de um perfil deve confirmar que o perfil
// pertence ao usuário autenticado — caso contrário qualquer um lê/altera
// dados alheios apenas trocando o id na URL/query (IDOR).
public abstract class ApiControllerBase : ControllerBase
{
    // Id do usuário (conta Google) extraído do claim "sub" do JWT.
    protected int? UsuarioId =>
        int.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out var id) ? id : null;

    // Retorna null se o perfil pertence ao usuário logado; senão o erro HTTP a devolver.
    // Uso: if (await GarantirDonoDoPerfilAsync(db, perfilId) is { } erro) return erro;
    protected async Task<IActionResult?> GarantirDonoDoPerfilAsync(AppDbContext db, int perfilId)
    {
        if (UsuarioId is not int usuarioId) return Unauthorized();
        var ehDono = await db.Perfis.AnyAsync(p => p.Id == perfilId && p.UsuarioId == usuarioId);
        return ehDono ? null : Forbid();
    }
}
