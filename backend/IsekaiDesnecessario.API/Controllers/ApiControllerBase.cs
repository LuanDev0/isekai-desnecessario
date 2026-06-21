using IsekaiDesnecessario.API.Data;
using IsekaiDesnecessario.API.Models;
using IsekaiDesnecessario.API.Services;
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

    // Trava por timer: enquanto TravadoAte > agora, a ativação não pode ser desativada.
    // Retorna o erro HTTP a devolver, ou null se está livre.
    protected IActionResult? Travado(DateTime? travadoAte) =>
        travadoAte is { } t && t > DateTime.UtcNow
            ? BadRequest($"Item travado até {t:dd/MM/yyyy}. Não pode desativar agora.")
            : null;

    // Classe RPG do perfil (null = sem classe definida). Usado no vínculo multi-classe.
    protected static Task<int?> ClasseDoPerfilAsync(AppDbContext db, int perfilId) =>
        db.Perfis.Where(p => p.Id == perfilId).Select(p => p.ClasseId).FirstOrDefaultAsync();

    // Quando um Admin edita um item global ainda Pendente de outro autor (moderador),
    // avisa o autor de que o item foi modificado antes de aprovar.
    protected async Task AvisarSeAdminModificouAsync(AppDbContext db, NotificacaoService notificacoes, IDefinicaoConteudo d)
    {
        if (d.Status == StatusConteudo.Pendente && d.CriadoPorUsuarioId is int autor && autor != UsuarioId
            && await ObterRoleAsync(db) == Role.Admin)
            await notificacoes.NotificarAsync(autor, "modificado", $"Seu item \"{d.Titulo}\" foi modificado pelo admin antes de aprovar.");
    }

    // Quem pode editar/excluir uma definição:
    //   Admin → sempre · não-autor → nunca · autor de conteúdo próprio → sempre ·
    //   autor (Moderador) de conteúdo global → só enquanto Pendente ("após aprovado, já era").
    protected async Task<bool> PodeMutarDefinicaoAsync(AppDbContext db, int? autorId, EscopoConteudo escopo, StatusConteudo status)
    {
        if (UsuarioId is not int uid) return false;
        if (await ObterRoleAsync(db) == Role.Admin) return true;
        if (autorId != uid) return false;
        if (escopo == EscopoConteudo.Proprio) return true;
        return status == StatusConteudo.Pendente;
    }

    // Papel de acesso do usuário logado. UsuarioId = 1 é sempre Admin (hardcoded).
    protected async Task<Role> ObterRoleAsync(AppDbContext db)
    {
        if (UsuarioId is not int usuarioId) return Role.Usuario;
        if (usuarioId == 1) return Role.Admin;
        return await db.Usuarios.Where(u => u.Id == usuarioId)
            .Select(u => u.Role).FirstOrDefaultAsync();
    }

    // Escopo/status de um item recém-criado conforme o papel + escolha do autor.
    // null = papel não pode criar nesse escopo.
    //   próprio (privado): VIP, Moderador e Admin → sempre Aprovado (sem aprovação)
    //   global  (catálogo): Admin → Aprovado · Moderador → Pendente · VIP → ✗
    //   Usuário comum → ✗ em qualquer escopo
    protected static (EscopoConteudo escopo, StatusConteudo status)? DefinirAutoria(Role role, bool proprio)
    {
        if (role == Role.Usuario) return null;
        if (proprio) return (EscopoConteudo.Proprio, StatusConteudo.Aprovado);
        return role switch
        {
            Role.Admin     => (EscopoConteudo.Global, StatusConteudo.Aprovado),
            Role.Moderador => (EscopoConteudo.Global, StatusConteudo.Pendente),
            _              => null, // VIP não cria conteúdo global
        };
    }
}
