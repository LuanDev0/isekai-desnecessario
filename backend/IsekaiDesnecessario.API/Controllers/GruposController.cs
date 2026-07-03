using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IsekaiDesnecessario.API.Data;
using IsekaiDesnecessario.API.Models;
using IsekaiDesnecessario.API.Services;

namespace IsekaiDesnecessario.API.Controllers;

// Grupos: criação/plano/cancelamento (Organizador), convites e membros.
// Conteúdo do grupo (hábitos/missões/recompensas) vive em GrupoConteudoController.
// Billing ainda não integrado — criação/upgrade liberados; quando o RevenueCat entrar,
// Create e Upgrade são os pontos de cobrança.
[ApiController]
[Route("api/grupos")]
[Authorize]
public class GruposController(AppDbContext db, GrupoService grupos, NotificacaoService notificacoes) : ApiControllerBase
{
    // Grupos visíveis ao perfil: onde é membro + os que a conta organiza.
    [HttpGet]
    public async Task<IActionResult> GetMeus([FromQuery] int perfilId)
    {
        if (await GarantirDonoDoPerfilAsync(db, perfilId) is { } erro) return erro;
        var uid = UsuarioId;
        var lista = await db.Grupos
            .Where(g => g.OrganizadorUsuarioId == uid || g.Membros.Any(m => m.PerfilId == perfilId))
            .OrderBy(g => g.Nome)
            .Select(g => new GrupoResumoDto(g.Id, g.Nome, g.Plano, g.MaxMembros, g.Ativo,
                g.OrganizadorUsuarioId == uid, g.Membros.Count,
                g.Membros.Any(m => m.PerfilId == perfilId),
                g.Membros.Where(m => m.PerfilId == perfilId).Select(m => (int?)m.XpGrupo).FirstOrDefault(),
                g.Membros.Where(m => m.PerfilId == perfilId).Select(m => (int?)m.MoedasGrupo).FirstOrDefault()))
            .AsNoTracking().ToListAsync();
        return Ok(lista);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CriarGrupoDto dto)
    {
        if (await GarantirDonoDoPerfilAsync(db, dto.PerfilId) is { } erro) return erro;
        if (string.IsNullOrWhiteSpace(dto.Nome)) return BadRequest("Nome do grupo é obrigatório.");
        if (!PlanosGrupo.Tamanhos.TryGetValue(dto.Plano, out var max)) return BadRequest("Plano inválido.");

        var grupo = new Grupo
        {
            Nome = dto.Nome.Trim(), Plano = dto.Plano, MaxMembros = max,
            OrganizadorUsuarioId = UsuarioId!.Value,
        };
        grupo.Membros.Add(new GrupoMembro { PerfilId = dto.PerfilId });
        db.Grupos.Add(grupo);
        await db.SaveChangesAsync();

        grupos.RegistrarFeed(grupo.Id, "grupo", $"Grupo \"{grupo.Nome}\" criado — plano {grupo.Plano} ({max} vagas).");
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetDetalhe), new { id = grupo.Id, perfilId = dto.PerfilId },
            new GrupoResumoDto(grupo.Id, grupo.Nome, grupo.Plano, grupo.MaxMembros, grupo.Ativo, true, 1, true, 0, 0));
    }

    // Detalhe completo: membros/ranking, conteúdo com estado do membro, feed e convites (organizador).
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDetalhe(int id, [FromQuery] int perfilId)
    {
        if (await GarantirDonoDoPerfilAsync(db, perfilId) is { } erro) return erro;
        var uid = UsuarioId;
        var grupo = await db.Grupos.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
        if (grupo is null) return NotFound();

        var souOrganizador = grupo.OrganizadorUsuarioId == uid;
        var membro = await db.GrupoMembros.AsNoTracking()
            .FirstOrDefaultAsync(m => m.GrupoId == id && m.PerfilId == perfilId);
        if (!souOrganizador && membro is null) return Forbid();
        if (!grupo.Ativo && !souOrganizador)
            return BadRequest("Grupo desativado pelo organizador.");

        var orgId = grupo.OrganizadorUsuarioId;
        var membros = await db.GrupoMembros
            .Where(m => m.GrupoId == id)
            .OrderByDescending(m => m.XpGrupo)
            .Select(m => new GrupoMembroDto(m.Id, m.PerfilId, m.Perfil!.Nome, m.Perfil.FotoUrl,
                m.XpGrupo, m.MoedasGrupo, m.Perfil.UsuarioId == orgId, m.EntrouEm))
            .AsNoTracking().ToListAsync();

        var membroId = membro?.Id ?? 0;
        var habitosRaw = await db.GrupoHabitos
            .Where(h => h.GrupoId == id)
            .Select(h => new { h.Id, h.Habito, h.Xp, h.Frequencia,
                UltimaExecucao = h.Execucoes.Where(x => x.GrupoMembroId == membroId)
                    .Max(x => (DateTime?)x.ExecutadoEm) })
            .AsNoTracking().ToListAsync();
        var habitos = habitosRaw
            .Select(h => new GrupoHabitoDto(h.Id, h.Habito, h.Xp, h.Frequencia, h.UltimaExecucao,
                GrupoService.EstaDisponivel(h.Frequencia, h.UltimaExecucao)))
            .ToList();

        var missoes = await db.GrupoMissoes
            .Where(m => m.GrupoId == id)
            .Select(m => new GrupoMissaoDto(m.Id, m.Titulo, m.RecompensaXp, m.RecompensaMoedas, m.DataLimite,
                m.Conclusoes.Any(c => c.GrupoMembroId == membroId),
                m.Conclusoes.Where(c => c.GrupoMembroId == membroId).Select(c => (DateTime?)c.ConcluidaEm).FirstOrDefault(),
                m.Conclusoes.Count))
            .AsNoTracking().ToListAsync();

        var recompensas = await db.GrupoRecompensas
            .Where(r => r.GrupoId == id)
            .Select(r => new GrupoRecompensaDto(r.Id, r.Nome, r.Custo,
                r.Resgates.Count(x => x.GrupoMembroId == membroId)))
            .AsNoTracking().ToListAsync();

        var feed = await db.GrupoFeedEventos
            .Where(f => f.GrupoId == id)
            .OrderByDescending(f => f.CriadoEm).Take(30)
            .Select(f => new GrupoFeedDto(f.Tipo, f.Mensagem, f.CriadoEm))
            .AsNoTracking().ToListAsync();

        List<GrupoConviteDto>? convites = null;
        if (souOrganizador)
            convites = await db.GrupoConvites
                .Where(c => c.GrupoId == id && c.Status == "Pendente")
                .Select(c => new GrupoConviteDto(c.Id, c.Usuario!.Email, c.CriadoEm))
                .AsNoTracking().ToListAsync();

        return Ok(new GrupoDetalheDto(grupo.Id, grupo.Nome, grupo.Plano, grupo.MaxMembros, grupo.Ativo,
            souOrganizador, grupo.CriadoEm,
            membro is null ? null : new GrupoMeuSaldoDto(membro.XpGrupo, membro.MoedasGrupo),
            membros, habitos, missoes, recompensas, feed, convites));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, AtualizarGrupoDto dto)
    {
        if (UsuarioId is not int uid) return Unauthorized();
        var grupo = await grupos.ObterGrupoDoOrganizadorAsync(id, uid);
        if (grupo is null) return NotFound();
        if (string.IsNullOrWhiteSpace(dto.Nome)) return BadRequest("Nome do grupo é obrigatório.");
        grupo.Nome = dto.Nome.Trim();
        await db.SaveChangesAsync();
        return Ok();
    }

    // Upgrade de plano — só para plano maior (downgrade não existe).
    [HttpPost("{id}/upgrade")]
    public async Task<IActionResult> Upgrade(int id, AtualizarPlanoDto dto)
    {
        if (UsuarioId is not int uid) return Unauthorized();
        var grupo = await grupos.ObterGrupoDoOrganizadorAsync(id, uid);
        if (grupo is null) return NotFound();
        if (!PlanosGrupo.Tamanhos.TryGetValue(dto.Plano, out var max)) return BadRequest("Plano inválido.");
        if (max <= grupo.MaxMembros) return BadRequest("Só é possível migrar para um plano maior.");

        grupo.Plano = dto.Plano;
        grupo.MaxMembros = max;
        grupos.RegistrarFeed(id, "grupo", $"Plano atualizado para {dto.Plano} ({max} vagas).");
        await db.SaveChangesAsync();
        return Ok();
    }

    // Cancela a assinatura: membros perdem acesso na hora e são notificados.
    [HttpPost("{id}/cancelar")]
    public async Task<IActionResult> Cancelar(int id)
    {
        if (UsuarioId is not int uid) return Unauthorized();
        var grupo = await grupos.ObterGrupoDoOrganizadorAsync(id, uid);
        if (grupo is null) return NotFound();
        if (!grupo.Ativo) return BadRequest("Grupo já está desativado.");

        grupo.Ativo = false;
        await grupos.NotificarMembrosAsync(id, "grupo",
            $"O grupo \"{grupo.Nome}\" foi desativado pelo organizador. Seu acesso foi suspenso.", excetoUsuarioId: uid);
        grupos.RegistrarFeed(id, "grupo", "Grupo desativado pelo organizador.");
        await db.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("{id}/reativar")]
    public async Task<IActionResult> Reativar(int id)
    {
        if (UsuarioId is not int uid) return Unauthorized();
        var grupo = await grupos.ObterGrupoDoOrganizadorAsync(id, uid);
        if (grupo is null) return NotFound();
        if (grupo.Ativo) return BadRequest("Grupo já está ativo.");

        grupo.Ativo = true;
        await grupos.NotificarMembrosAsync(id, "grupo",
            $"O grupo \"{grupo.Nome}\" foi reativado. Seu acesso voltou!", excetoUsuarioId: uid);
        grupos.RegistrarFeed(id, "grupo", "Grupo reativado pelo organizador.");
        await db.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (UsuarioId is not int uid) return Unauthorized();
        var grupo = await grupos.ObterGrupoDoOrganizadorAsync(id, uid);
        if (grupo is null) return NotFound();

        await grupos.NotificarMembrosAsync(id, "grupo",
            $"O grupo \"{grupo.Nome}\" foi excluído pelo organizador.", excetoUsuarioId: uid);
        db.Grupos.Remove(grupo); // membros, conteúdo, feed e convites caem em cascata
        await db.SaveChangesAsync();
        return NoContent();
    }

    // ════════════════════ Convites ════════════════════

    // Organizador convida uma conta pelo e-mail. O destinatário aceita com o perfil que quiser.
    [HttpPost("{id}/convites")]
    public async Task<IActionResult> Convidar(int id, ConvidarDto dto)
    {
        if (UsuarioId is not int uid) return Unauthorized();
        var grupo = await grupos.ObterGrupoDoOrganizadorAsync(id, uid);
        if (grupo is null) return NotFound();
        if (!grupo.Ativo) return BadRequest("Grupo desativado — reative antes de convidar.");

        var totalMembros = await db.GrupoMembros.CountAsync(m => m.GrupoId == id);
        if (totalMembros >= grupo.MaxMembros)
            return BadRequest($"Grupo lotado ({grupo.MaxMembros} membros). Faça upgrade do plano para convidar mais.");

        var email = dto.Email.Trim().ToLower();
        var convidado = await db.Usuarios.FirstOrDefaultAsync(u => u.Email.ToLower() == email);
        if (convidado is null) return NotFound("Nenhuma conta encontrada com esse e-mail.");

        var jaMembro = await db.GrupoMembros.AnyAsync(m => m.GrupoId == id && m.Perfil!.UsuarioId == convidado.Id);
        if (jaMembro) return BadRequest("Essa conta já está no grupo.");

        var jaConvidado = await db.GrupoConvites
            .AnyAsync(c => c.GrupoId == id && c.UsuarioId == convidado.Id && c.Status == "Pendente");
        if (jaConvidado) return BadRequest("Convite já enviado para essa conta.");

        db.GrupoConvites.Add(new GrupoConvite { GrupoId = id, UsuarioId = convidado.Id });
        await notificacoes.NotificarAsync(convidado.Id, "convite",
            $"Você foi convidado para o grupo \"{grupo.Nome}\". Veja em Grupos.");
        return Ok();
    }

    // Convites pendentes da conta logada.
    [HttpGet("convites")]
    public async Task<IActionResult> MeusConvites()
    {
        if (UsuarioId is not int uid) return Unauthorized();
        var lista = await db.GrupoConvites
            .Where(c => c.UsuarioId == uid && c.Status == "Pendente" && c.Grupo!.Ativo)
            .OrderByDescending(c => c.CriadoEm)
            .Select(c => new ConvitePendenteDto(c.Id, c.GrupoId, c.Grupo!.Nome, c.Grupo.Plano,
                db.GrupoMembros.Count(m => m.GrupoId == c.GrupoId), c.Grupo.MaxMembros, c.CriadoEm))
            .AsNoTracking().ToListAsync();
        return Ok(lista);
    }

    [HttpPost("convites/{conviteId}/aceitar")]
    public async Task<IActionResult> AceitarConvite(int conviteId, [FromQuery] int perfilId)
    {
        if (await GarantirDonoDoPerfilAsync(db, perfilId) is { } erro) return erro;
        var uid = UsuarioId;
        var convite = await db.GrupoConvites.Include(c => c.Grupo)
            .FirstOrDefaultAsync(c => c.Id == conviteId && c.UsuarioId == uid && c.Status == "Pendente");
        if (convite?.Grupo is null) return NotFound();
        if (!convite.Grupo.Ativo) return BadRequest("Grupo desativado pelo organizador.");

        var totalMembros = await db.GrupoMembros.CountAsync(m => m.GrupoId == convite.GrupoId);
        if (totalMembros >= convite.Grupo.MaxMembros) return BadRequest("Grupo lotado.");

        var jaMembro = await db.GrupoMembros.AnyAsync(m => m.GrupoId == convite.GrupoId && m.PerfilId == perfilId);
        if (jaMembro) return BadRequest("Esse perfil já está no grupo.");

        convite.Status = "Aceito";
        db.GrupoMembros.Add(new GrupoMembro { GrupoId = convite.GrupoId, PerfilId = perfilId });
        var nomePerfil = await db.Perfis.Where(p => p.Id == perfilId).Select(p => p.Nome).FirstAsync();
        grupos.RegistrarFeed(convite.GrupoId, "entrou", $"{nomePerfil} entrou no grupo.");
        await db.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("convites/{conviteId}/recusar")]
    public async Task<IActionResult> RecusarConvite(int conviteId)
    {
        if (UsuarioId is not int uid) return Unauthorized();
        var convite = await db.GrupoConvites
            .FirstOrDefaultAsync(c => c.Id == conviteId && c.UsuarioId == uid && c.Status == "Pendente");
        if (convite is null) return NotFound();
        convite.Status = "Recusado";
        await db.SaveChangesAsync();
        return Ok();
    }

    // ════════════════════ Membros ════════════════════

    // Membro sai por conta própria. O Organizador não sai — cancela ou exclui o grupo.
    [HttpPost("{id}/sair")]
    public async Task<IActionResult> Sair(int id, [FromQuery] int perfilId)
    {
        if (await GarantirDonoDoPerfilAsync(db, perfilId) is { } erro) return erro;
        var membro = await grupos.ObterMembroAsync(id, perfilId);
        if (membro is null) return NotFound();

        var orgId = await db.Grupos.Where(g => g.Id == id).Select(g => g.OrganizadorUsuarioId).FirstAsync();
        if (orgId == UsuarioId)
            return BadRequest("O organizador não pode sair do próprio grupo — cancele ou exclua o grupo.");

        var nomePerfil = await db.Perfis.Where(p => p.Id == perfilId).Select(p => p.Nome).FirstAsync();
        db.GrupoMembros.Remove(membro); // execuções/conclusões/resgates caem em cascata
        grupos.RegistrarFeed(id, "saiu", $"{nomePerfil} saiu do grupo.");
        await db.SaveChangesAsync();
        return Ok();
    }

    // Organizador remove um membro; a conta removida é notificada.
    [HttpDelete("{id}/membros/{membroId}")]
    public async Task<IActionResult> RemoverMembro(int id, int membroId)
    {
        if (UsuarioId is not int uid) return Unauthorized();
        var grupo = await grupos.ObterGrupoDoOrganizadorAsync(id, uid);
        if (grupo is null) return NotFound();

        var membro = await db.GrupoMembros.Include(m => m.Perfil)
            .FirstOrDefaultAsync(m => m.Id == membroId && m.GrupoId == id);
        if (membro?.Perfil is null) return NotFound();
        if (membro.Perfil.UsuarioId == uid)
            return BadRequest("O organizador não pode remover a si mesmo.");

        if (membro.Perfil.UsuarioId is int contaRemovida)
            await notificacoes.NotificarAsync(contaRemovida, "grupo",
                $"Você foi removido do grupo \"{grupo.Nome}\" pelo organizador.");
        grupos.RegistrarFeed(id, "removido", $"{membro.Perfil.Nome} foi removido do grupo.");
        db.GrupoMembros.Remove(membro);
        await db.SaveChangesAsync();
        return Ok();
    }
}

// ── DTOs ─────────────────────────────────────────────────
public record CriarGrupoDto(string Nome, string Plano, int PerfilId);
public record AtualizarGrupoDto(string Nome);
public record AtualizarPlanoDto(string Plano);
public record ConvidarDto(string Email);

public record GrupoResumoDto(int Id, string Nome, string Plano, int MaxMembros, bool Ativo,
    bool Organizador, int TotalMembros, bool Membro, int? XpGrupo, int? MoedasGrupo);

public record GrupoMeuSaldoDto(int XpGrupo, int MoedasGrupo);
public record GrupoMembroDto(int Id, int PerfilId, string Nome, string? FotoUrl,
    int XpGrupo, int MoedasGrupo, bool Organizador, DateTime EntrouEm);
public record GrupoHabitoDto(int Id, string Habito, int Xp, string Frequencia,
    DateTime? UltimaExecucao, bool Disponivel);
public record GrupoMissaoDto(int Id, string Titulo, int RecompensaXp, int RecompensaMoedas,
    DateTime? DataLimite, bool Concluida, DateTime? ConcluidaEm, int TotalConclusoes);
public record GrupoRecompensaDto(int Id, string Nome, int Custo, int MeusResgates);
public record GrupoFeedDto(string Tipo, string Mensagem, DateTime CriadoEm);
public record GrupoConviteDto(int Id, string Email, DateTime CriadoEm);
public record ConvitePendenteDto(int Id, int GrupoId, string GrupoNome, string Plano,
    int TotalMembros, int MaxMembros, DateTime CriadoEm);

public record GrupoDetalheDto(int Id, string Nome, string Plano, int MaxMembros, bool Ativo,
    bool Organizador, DateTime CriadoEm, GrupoMeuSaldoDto? Membro,
    List<GrupoMembroDto> Membros, List<GrupoHabitoDto> Habitos, List<GrupoMissaoDto> Missoes,
    List<GrupoRecompensaDto> Recompensas, List<GrupoFeedDto> Feed, List<GrupoConviteDto>? Convites);
