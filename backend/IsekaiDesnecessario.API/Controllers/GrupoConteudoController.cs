using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IsekaiDesnecessario.API.Data;
using IsekaiDesnecessario.API.Models;
using IsekaiDesnecessario.API.Services;

namespace IsekaiDesnecessario.API.Controllers;

// Conteúdo dos grupos: hábitos, missões e recompensas exclusivos.
// Só o Organizador cria/edita/exclui (VIP/Admin/Moderador não têm privilégio aqui);
// qualquer membro completa/resgata. XP e moedas são do grupo (GrupoMembro), nunca do perfil.
[ApiController]
[Route("api/grupos")]
[Authorize]
public class GrupoConteudoController(AppDbContext db, GrupoService grupos) : ApiControllerBase
{
    private static readonly string[] FrequenciasValidas = ["Diário", "Semanal", "Mensal", "Livre"];

    // ════════════════════ Hábitos do grupo ════════════════════

    [HttpPost("{grupoId}/habitos")]
    public async Task<IActionResult> CriarHabito(int grupoId, GrupoHabitoForm dto)
    {
        if (UsuarioId is not int uid) return Unauthorized();
        var grupo = await grupos.ObterGrupoDoOrganizadorAsync(grupoId, uid);
        if (grupo is null) return NotFound();
        if (ValidarHabito(dto) is { } invalido) return invalido;

        var habito = new GrupoHabito { GrupoId = grupoId, Habito = dto.Habito.Trim(), Xp = dto.Xp, Frequencia = dto.Frequencia };
        db.GrupoHabitos.Add(habito);
        grupos.RegistrarFeed(grupoId, "conteudo", $"Novo hábito no grupo: \"{habito.Habito}\" (+{habito.Xp} XP).");
        await db.SaveChangesAsync();
        return Ok(habito);
    }

    [HttpPut("habitos/{id}")]
    public async Task<IActionResult> EditarHabito(int id, GrupoHabitoForm dto)
    {
        if (UsuarioId is not int uid) return Unauthorized();
        var habito = await db.GrupoHabitos.FirstOrDefaultAsync(h =>
            h.Id == id && h.Grupo!.OrganizadorUsuarioId == uid);
        if (habito is null) return NotFound();
        if (ValidarHabito(dto) is { } invalido) return invalido;

        habito.Habito = dto.Habito.Trim();
        habito.Xp = dto.Xp;
        habito.Frequencia = dto.Frequencia;
        await db.SaveChangesAsync();
        return Ok(habito);
    }

    [HttpDelete("habitos/{id}")]
    public async Task<IActionResult> ExcluirHabito(int id)
    {
        if (UsuarioId is not int uid) return Unauthorized();
        var habito = await db.GrupoHabitos.FirstOrDefaultAsync(h =>
            h.Id == id && h.Grupo!.OrganizadorUsuarioId == uid);
        if (habito is null) return NotFound();
        db.GrupoHabitos.Remove(habito); // execuções caem em cascata
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("habitos/{id}/completar")]
    public async Task<IActionResult> CompletarHabito(int id, [FromQuery] int perfilId)
    {
        if (await GarantirDonoDoPerfilAsync(db, perfilId) is { } erro) return erro;
        var habito = await db.GrupoHabitos.Include(h => h.Grupo).FirstOrDefaultAsync(h => h.Id == id);
        if (habito?.Grupo is null) return NotFound();
        if (!habito.Grupo.Ativo) return BadRequest("Grupo desativado pelo organizador.");

        var membro = await grupos.ObterMembroAsync(habito.GrupoId, perfilId);
        if (membro is null) return Forbid();

        var ultima = await db.GrupoHabitoExecucoes
            .Where(x => x.GrupoHabitoId == id && x.GrupoMembroId == membro.Id)
            .MaxAsync(x => (DateTime?)x.ExecutadoEm);
        if (!GrupoService.EstaDisponivel(habito.Frequencia, ultima))
            return BadRequest("Hábito ainda em cooldown.");

        db.GrupoHabitoExecucoes.Add(new GrupoHabitoExecucao { GrupoHabitoId = id, GrupoMembroId = membro.Id });
        membro.XpGrupo += habito.Xp;
        var nomePerfil = await db.Perfis.Where(p => p.Id == perfilId).Select(p => p.Nome).FirstAsync();
        grupos.RegistrarFeed(habito.GrupoId, "habito", $"{nomePerfil} completou \"{habito.Habito}\" +{habito.Xp} XP");
        await db.SaveChangesAsync();
        return Ok(new GrupoMeuSaldoDto(membro.XpGrupo, membro.MoedasGrupo));
    }

    // ════════════════════ Missões do grupo ════════════════════

    [HttpPost("{grupoId}/missoes")]
    public async Task<IActionResult> CriarMissao(int grupoId, GrupoMissaoForm dto)
    {
        if (UsuarioId is not int uid) return Unauthorized();
        var grupo = await grupos.ObterGrupoDoOrganizadorAsync(grupoId, uid);
        if (grupo is null) return NotFound();
        if (ValidarMissao(dto) is { } invalido) return invalido;

        var missao = new GrupoMissao
        {
            GrupoId = grupoId, Titulo = dto.Titulo.Trim(),
            RecompensaXp = dto.RecompensaXp, RecompensaMoedas = dto.RecompensaMoedas,
            DataLimite = dto.DataLimite.HasValue ? DateTime.SpecifyKind(dto.DataLimite.Value, DateTimeKind.Utc) : null,
        };
        db.GrupoMissoes.Add(missao);
        grupos.RegistrarFeed(grupoId, "conteudo", $"Nova missão no grupo: \"{missao.Titulo}\".");
        await db.SaveChangesAsync();
        return Ok(missao);
    }

    [HttpPut("missoes/{id}")]
    public async Task<IActionResult> EditarMissao(int id, GrupoMissaoForm dto)
    {
        if (UsuarioId is not int uid) return Unauthorized();
        var missao = await db.GrupoMissoes.FirstOrDefaultAsync(m =>
            m.Id == id && m.Grupo!.OrganizadorUsuarioId == uid);
        if (missao is null) return NotFound();
        if (ValidarMissao(dto) is { } invalido) return invalido;

        missao.Titulo = dto.Titulo.Trim();
        missao.RecompensaXp = dto.RecompensaXp;
        missao.RecompensaMoedas = dto.RecompensaMoedas;
        missao.DataLimite = dto.DataLimite.HasValue ? DateTime.SpecifyKind(dto.DataLimite.Value, DateTimeKind.Utc) : null;
        await db.SaveChangesAsync();
        return Ok(missao);
    }

    [HttpDelete("missoes/{id}")]
    public async Task<IActionResult> ExcluirMissao(int id)
    {
        if (UsuarioId is not int uid) return Unauthorized();
        var missao = await db.GrupoMissoes.FirstOrDefaultAsync(m =>
            m.Id == id && m.Grupo!.OrganizadorUsuarioId == uid);
        if (missao is null) return NotFound();
        db.GrupoMissoes.Remove(missao); // conclusões caem em cascata
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("missoes/{id}/completar")]
    public async Task<IActionResult> CompletarMissao(int id, [FromQuery] int perfilId)
    {
        if (await GarantirDonoDoPerfilAsync(db, perfilId) is { } erro) return erro;
        var missao = await db.GrupoMissoes.Include(m => m.Grupo).FirstOrDefaultAsync(m => m.Id == id);
        if (missao?.Grupo is null) return NotFound();
        if (!missao.Grupo.Ativo) return BadRequest("Grupo desativado pelo organizador.");
        if (missao.DataLimite is { } limite && limite < DateTime.UtcNow)
            return BadRequest("Prazo da missão encerrado.");

        var membro = await grupos.ObterMembroAsync(missao.GrupoId, perfilId);
        if (membro is null) return Forbid();

        var jaConcluiu = await db.GrupoMissaoConclusoes
            .AnyAsync(c => c.GrupoMissaoId == id && c.GrupoMembroId == membro.Id);
        if (jaConcluiu) return BadRequest("Missão já concluída.");

        db.GrupoMissaoConclusoes.Add(new GrupoMissaoConclusao { GrupoMissaoId = id, GrupoMembroId = membro.Id });
        membro.XpGrupo += missao.RecompensaXp;
        membro.MoedasGrupo += missao.RecompensaMoedas;
        var nomePerfil = await db.Perfis.Where(p => p.Id == perfilId).Select(p => p.Nome).FirstAsync();
        grupos.RegistrarFeed(missao.GrupoId, "missao",
            $"{nomePerfil} concluiu a missão \"{missao.Titulo}\" +{missao.RecompensaXp} XP");
        await db.SaveChangesAsync();
        return Ok(new GrupoMeuSaldoDto(membro.XpGrupo, membro.MoedasGrupo));
    }

    // ════════════════════ Recompensas do grupo ════════════════════

    [HttpPost("{grupoId}/recompensas")]
    public async Task<IActionResult> CriarRecompensa(int grupoId, GrupoRecompensaForm dto)
    {
        if (UsuarioId is not int uid) return Unauthorized();
        var grupo = await grupos.ObterGrupoDoOrganizadorAsync(grupoId, uid);
        if (grupo is null) return NotFound();
        if (ValidarRecompensa(dto) is { } invalido) return invalido;

        var recompensa = new GrupoRecompensa { GrupoId = grupoId, Nome = dto.Nome.Trim(), Custo = dto.Custo };
        db.GrupoRecompensas.Add(recompensa);
        grupos.RegistrarFeed(grupoId, "conteudo", $"Nova recompensa no grupo: \"{recompensa.Nome}\" ({recompensa.Custo} moedas).");
        await db.SaveChangesAsync();
        return Ok(recompensa);
    }

    [HttpPut("recompensas/{id}")]
    public async Task<IActionResult> EditarRecompensa(int id, GrupoRecompensaForm dto)
    {
        if (UsuarioId is not int uid) return Unauthorized();
        var recompensa = await db.GrupoRecompensas.FirstOrDefaultAsync(r =>
            r.Id == id && r.Grupo!.OrganizadorUsuarioId == uid);
        if (recompensa is null) return NotFound();
        if (ValidarRecompensa(dto) is { } invalido) return invalido;

        recompensa.Nome = dto.Nome.Trim();
        recompensa.Custo = dto.Custo;
        await db.SaveChangesAsync();
        return Ok(recompensa);
    }

    [HttpDelete("recompensas/{id}")]
    public async Task<IActionResult> ExcluirRecompensa(int id)
    {
        if (UsuarioId is not int uid) return Unauthorized();
        var recompensa = await db.GrupoRecompensas.FirstOrDefaultAsync(r =>
            r.Id == id && r.Grupo!.OrganizadorUsuarioId == uid);
        if (recompensa is null) return NotFound();
        db.GrupoRecompensas.Remove(recompensa); // resgates caem em cascata
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("recompensas/{id}/resgatar")]
    public async Task<IActionResult> ResgatarRecompensa(int id, [FromQuery] int perfilId)
    {
        if (await GarantirDonoDoPerfilAsync(db, perfilId) is { } erro) return erro;
        var recompensa = await db.GrupoRecompensas.Include(r => r.Grupo).FirstOrDefaultAsync(r => r.Id == id);
        if (recompensa?.Grupo is null) return NotFound();
        if (!recompensa.Grupo.Ativo) return BadRequest("Grupo desativado pelo organizador.");

        var membro = await grupos.ObterMembroAsync(recompensa.GrupoId, perfilId);
        if (membro is null) return Forbid();
        if (membro.MoedasGrupo < recompensa.Custo)
            return BadRequest($"Moedas insuficientes — precisa de {recompensa.Custo}, tem {membro.MoedasGrupo}.");

        membro.MoedasGrupo -= recompensa.Custo;
        db.GrupoRecompensaResgates.Add(new GrupoRecompensaResgate { GrupoRecompensaId = id, GrupoMembroId = membro.Id });
        var nomePerfil = await db.Perfis.Where(p => p.Id == perfilId).Select(p => p.Nome).FirstAsync();
        grupos.RegistrarFeed(recompensa.GrupoId, "recompensa",
            $"{nomePerfil} resgatou \"{recompensa.Nome}\" (-{recompensa.Custo} moedas)");
        await db.SaveChangesAsync();
        return Ok(new GrupoMeuSaldoDto(membro.XpGrupo, membro.MoedasGrupo));
    }

    // ── Validações ───────────────────────────────────────
    private IActionResult? ValidarHabito(GrupoHabitoForm dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Habito)) return BadRequest("Nome do hábito é obrigatório.");
        if (dto.Xp is < 1 or > 9999) return BadRequest("XP deve estar entre 1 e 9999.");
        if (!FrequenciasValidas.Contains(dto.Frequencia)) return BadRequest("Frequência inválida.");
        return null;
    }

    private IActionResult? ValidarMissao(GrupoMissaoForm dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Titulo)) return BadRequest("Título da missão é obrigatório.");
        if (dto.RecompensaXp is < 0 or > 9999 || dto.RecompensaMoedas is < 0 or > 9999)
            return BadRequest("Recompensas devem estar entre 0 e 9999.");
        return null;
    }

    private IActionResult? ValidarRecompensa(GrupoRecompensaForm dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nome)) return BadRequest("Nome da recompensa é obrigatório.");
        if (dto.Custo is < 1 or > 99999) return BadRequest("Custo deve estar entre 1 e 99999.");
        return null;
    }
}

// ── DTOs ─────────────────────────────────────────────────
public record GrupoHabitoForm(string Habito, int Xp, string Frequencia);
public record GrupoMissaoForm(string Titulo, int RecompensaXp, int RecompensaMoedas, DateTime? DataLimite);
public record GrupoRecompensaForm(string Nome, int Custo);
