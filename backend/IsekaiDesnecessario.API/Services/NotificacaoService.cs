using Microsoft.EntityFrameworkCore;
using IsekaiDesnecessario.API.Data;
using IsekaiDesnecessario.API.Models;

namespace IsekaiDesnecessario.API.Services;

// Cria notificações in-app (sininho) para os eventos do fluxo de aprovação.
public class NotificacaoService(AppDbContext db)
{
    // Notifica uma conta específica (ex.: o moderador autor).
    public async Task NotificarAsync(int usuarioId, string tipo, string mensagem)
    {
        db.Notificacoes.Add(new Notificacao { UsuarioId = usuarioId, Tipo = tipo, Mensagem = mensagem });
        await db.SaveChangesAsync();
    }

    // Notifica todos os admins (Role = Admin ou a conta 1, admin inicial hardcoded).
    public async Task NotificarAdminsAsync(string tipo, string mensagem)
    {
        var admins = await db.Usuarios
            .Where(u => u.Role == Role.Admin || u.Id == 1)
            .Select(u => u.Id).ToListAsync();
        foreach (var id in admins)
            db.Notificacoes.Add(new Notificacao { UsuarioId = id, Tipo = tipo, Mensagem = mensagem });
        await db.SaveChangesAsync();
    }
}
