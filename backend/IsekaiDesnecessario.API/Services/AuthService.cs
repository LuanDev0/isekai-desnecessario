using Google.Apis.Auth;
using IsekaiDesnecessario.API.Data;
using IsekaiDesnecessario.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IsekaiDesnecessario.API.Services;

public class AuthService(AppDbContext db, IConfiguration config, ILogger<AuthService> logger)
{
    // ── Google OAuth ──────────────────────────────────────────────────────────
    public async Task<(string jwt, Usuario usuario, List<Perfil> perfis)> LoginComGoogleAsync(string idToken)
    {
        var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = [config["Google:ClientId"]!]
        });

        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.GoogleId == payload.Subject)
                   ?? await db.Usuarios.FirstOrDefaultAsync(u => u.Email == payload.Email);

        if (usuario is null)
        {
            usuario = new Usuario
            {
                GoogleId        = payload.Subject,
                Email           = payload.Email,
                EmailVerificado = payload.EmailVerified,
                Nome            = payload.Name ?? payload.Email,
                FotoUrl         = payload.Picture,
            };
            db.Usuarios.Add(usuario);
        }
        else
        {
            usuario.GoogleId    ??= payload.Subject;
            usuario.UltimoLogin   = DateTime.UtcNow;
            usuario.Nome          = payload.Name ?? usuario.Nome;
            usuario.FotoUrl       = payload.Picture ?? usuario.FotoUrl;
            if (payload.EmailVerified) usuario.EmailVerificado = true;
        }

        await db.SaveChangesAsync();

        var perfis = await db.Perfis.Where(p => p.UsuarioId == usuario.Id).ToListAsync();
        logger.LogInformation("Login Google: usuário {UsuarioId} ({Email})", usuario.Id, usuario.Email);
        return (GerarJwt(usuario), usuario, perfis);
    }

    // ── Registro email/senha ──────────────────────────────────────────────────
    public async Task<(string jwt, Usuario usuario, List<Perfil> perfis)> RegistrarAsync(string nome, string email, string senha)
    {
        if (await db.Usuarios.AnyAsync(u => u.Email == email))
            throw new InvalidOperationException("E-mail já cadastrado.");

        ValidarSenha(senha);

        var usuario = new Usuario
        {
            Nome            = nome.Trim(),
            Email           = email.Trim().ToLowerInvariant(),
            EmailVerificado = false,
            SenhaHash       = BCrypt.Net.BCrypt.HashPassword(senha, workFactor: 12),
        };

        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();

        logger.LogInformation("Novo usuário registrado: {UsuarioId} ({Email})", usuario.Id, usuario.Email);
        return (GerarJwt(usuario), usuario, []);
    }

    // ── Login email/senha ─────────────────────────────────────────────────────
    public async Task<(string jwt, Usuario usuario, List<Perfil> perfis)> LoginAsync(string email, string senha)
    {
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Email == email.Trim().ToLowerInvariant());

        if (usuario is null || usuario.SenhaHash is null || !BCrypt.Net.BCrypt.Verify(senha, usuario.SenhaHash))
        {
            logger.LogWarning("Tentativa de login falhou para e-mail {Email}", email.Trim().ToLowerInvariant());
            throw new UnauthorizedAccessException("E-mail ou senha incorretos.");
        }

        usuario.UltimoLogin = DateTime.UtcNow;
        await db.SaveChangesAsync();

        var perfis = await db.Perfis.Where(p => p.UsuarioId == usuario.Id).ToListAsync();
        logger.LogInformation("Login: usuário {UsuarioId} ({Email})", usuario.Id, usuario.Email);
        return (GerarJwt(usuario), usuario, perfis);
    }

    // ── JWT ───────────────────────────────────────────────────────────────────
    private string GerarJwt(Usuario usuario)
    {
        var key    = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Secret"]!));
        var creds  = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var horas  = double.Parse(config["Jwt:ExpiresHours"] ?? "168", CultureInfo.InvariantCulture);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim("nome",                        usuario.Nome),
        };

        var token = new JwtSecurityToken(
            issuer:             config["Jwt:Issuer"],
            audience:           config["Jwt:Audience"],
            claims:             claims,
            expires:            DateTime.UtcNow.AddHours(horas),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static void ValidarSenha(string senha)
    {
        if (senha.Length < 8)
            throw new ArgumentException("A senha deve ter pelo menos 8 caracteres.");
        if (!senha.Any(char.IsUpper))
            throw new ArgumentException("A senha deve conter pelo menos uma letra maiúscula.");
        if (!senha.Any(char.IsDigit))
            throw new ArgumentException("A senha deve conter pelo menos um número.");
    }
}
