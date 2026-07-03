using Microsoft.EntityFrameworkCore;
using IsekaiDesnecessario.API.Data;
using IsekaiDesnecessario.API.Models;

namespace IsekaiDesnecessario.API.Services;

// Helpers compartilhados pelos controllers de grupo: feed, checagens de papel e frequência.
public class GrupoService(AppDbContext db)
{
    // Adiciona evento à timeline do grupo (não salva — SaveChanges fica com o chamador).
    public void RegistrarFeed(int grupoId, string tipo, string mensagem) =>
        db.GrupoFeedEventos.Add(new GrupoFeedEvento { GrupoId = grupoId, Tipo = tipo, Mensagem = mensagem });

    // Grupo administrado pela conta (Organizador) — null se não existe ou não é o organizador.
    public Task<Grupo?> ObterGrupoDoOrganizadorAsync(int grupoId, int usuarioId) =>
        db.Grupos.FirstOrDefaultAsync(g => g.Id == grupoId && g.OrganizadorUsuarioId == usuarioId);

    // Participação de um perfil num grupo — null se não é membro.
    public Task<GrupoMembro?> ObterMembroAsync(int grupoId, int perfilId) =>
        db.GrupoMembros.FirstOrDefaultAsync(m => m.GrupoId == grupoId && m.PerfilId == perfilId);

    // Notifica a conta de cada membro do grupo (perfis convidados, sem conta, ficam de fora).
    public async Task NotificarMembrosAsync(int grupoId, string tipo, string mensagem, int? excetoUsuarioId = null)
    {
        var contas = await db.GrupoMembros
            .Where(m => m.GrupoId == grupoId && m.Perfil!.UsuarioId != null && m.Perfil.UsuarioId != excetoUsuarioId)
            .Select(m => m.Perfil!.UsuarioId!.Value)
            .Distinct().ToListAsync();
        foreach (var usuarioId in contas)
            db.Notificacoes.Add(new Notificacao { UsuarioId = usuarioId, Tipo = tipo, Mensagem = mensagem });
    }

    // Mesma régua de frequência dos hábitos do jogo principal (HabitosController).
    public static bool EstaDisponivel(string frequencia, DateTime? ultimaExecucao)
    {
        if (ultimaExecucao is null) return true;
        var ultima = ultimaExecucao.Value;
        var agora  = DateTime.UtcNow;
        return frequencia switch
        {
            "Diário"  => ultima.Date < agora.Date,
            "Semanal" => ultima < InicioSemanaAtual(agora),
            "Mensal"  => ultima < new DateTime(agora.Year, agora.Month, 1),
            _         => true
        };
    }

    private static DateTime InicioSemanaAtual(DateTime agora)
    {
        int diff = (int)agora.DayOfWeek - (int)DayOfWeek.Monday;
        if (diff < 0) diff += 7;
        return agora.Date.AddDays(-diff);
    }
}
