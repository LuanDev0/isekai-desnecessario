using Google.Apis.Auth;
using IsekaiDesnecessario.API.Data;
using IsekaiDesnecessario.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IsekaiDesnecessario.API.Services;

public class AuthService(AppDbContext db, IConfiguration config)
{
    public async Task<(string jwt, Usuario usuario, List<Perfil> perfis)> LoginComGoogle(string idToken)
    {
        var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = [config["Google:ClientId"]!]
        });

        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.GoogleId == payload.Subject);

        if (usuario is null)
        {
            usuario = new Usuario
            {
                GoogleId         = payload.Subject,
                Email            = payload.Email,
                EmailVerificado  = payload.EmailVerified,
                Nome             = payload.Name ?? payload.Email,
                FotoUrl          = payload.Picture,
            };
            db.Usuarios.Add(usuario);
        }
        else
        {
            usuario.UltimoLogin = DateTime.UtcNow;
            usuario.Nome        = payload.Name ?? usuario.Nome;
            usuario.FotoUrl     = payload.Picture ?? usuario.FotoUrl;
        }

        await db.SaveChangesAsync();

        var perfis = await db.Perfis.Where(p => p.UsuarioId == usuario.Id).ToListAsync();
        var jwt    = GerarJwt(usuario);

        return (jwt, usuario, perfis);
    }

    private string GerarJwt(Usuario usuario)
    {
        var key    = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Secret"]!));
        var creds  = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var horas  = double.Parse(config["Jwt:ExpiresHours"] ?? "168");
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim("nome",                        usuario.Nome),
        };

        var token = new JwtSecurityToken(
            issuer:            config["Jwt:Issuer"],
            audience:          config["Jwt:Audience"],
            claims:            claims,
            expires:           DateTime.UtcNow.AddHours(horas),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
