using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoloLeveling.API.Data;
using SoloLeveling.API.Models;

namespace SoloLeveling.API.Controllers;

[ApiController]
[Route("api/experimentos")]
public class ExperimentosController(AppDbContext db) : ControllerBase
{
    // GET /api/experimentos?perfilId=X
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] int perfilId)
    {
        var lista = await db.Experimentos
            .Include(e => e.Dias)
            .Where(e => e.PerfilId == perfilId)
            .OrderByDescending(e => e.DataInicio)
            .ToListAsync();
        return Ok(lista);
    }

    // POST /api/experimentos
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarExperimentoDto dto)
    {
        var exp = new Experimento
        {
            PerfilId    = dto.PerfilId,
            Titulo      = dto.Titulo,
            Descricao   = dto.Descricao,
            DuracaoDias = dto.DuracaoDias,
            DataInicio  = DateTime.Today,
            Ativo       = true,
            Convertido  = false,
        };
        db.Experimentos.Add(exp);
        await db.SaveChangesAsync();
        return Ok(exp);
    }

    // DELETE /api/experimentos/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var exp = await db.Experimentos.FindAsync(id);
        if (exp is null) return NotFound();
        db.Experimentos.Remove(exp);
        await db.SaveChangesAsync();
        return NoContent();
    }

    // POST /api/experimentos/{id}/dia — marca dia como feito
    [HttpPost("{id}/dia")]
    public async Task<IActionResult> MarcarDia(int id)
    {
        var exp = await db.Experimentos.Include(e => e.Dias).FirstOrDefaultAsync(e => e.Id == id);
        if (exp is null) return NotFound();

        var hoje = DateTime.Today;
        if (exp.Dias.Any(d => d.Data.Date == hoje))
            return BadRequest("Já marcado hoje.");

        var dia = new ExperimentoDia { ExperimentoId = id, Data = hoje };
        db.ExperimentosDia.Add(dia);

        // Se completou a duração, encerra
        if (exp.Dias.Count + 1 >= exp.DuracaoDias)
            exp.Ativo = false;

        await db.SaveChangesAsync();
        return Ok(exp);
    }

    // POST /api/experimentos/{id}/converter — vira bom hábito
    [HttpPost("{id}/converter")]
    public async Task<IActionResult> Converter(int id, [FromQuery] int perfilId)
    {
        var exp = await db.Experimentos.FindAsync(id);
        if (exp is null) return NotFound();

        exp.Ativo      = false;
        exp.Convertido = true;

        var habito = new BomHabito
        {
            PerfilId  = perfilId,
            Habito    = exp.Titulo,
            Xp        = 10,
            Frequencia = "Diária",
            Streak    = 0,
        };
        db.BonsHabitos.Add(habito);
        await db.SaveChangesAsync();
        return Ok(new { experimento = exp, habito });
    }
}

public record CriarExperimentoDto(int PerfilId, string Titulo, string Descricao, int DuracaoDias);
