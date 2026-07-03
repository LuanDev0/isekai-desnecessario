using System.Text.Json.Serialization;

namespace IsekaiDesnecessario.API.Models;

// Missão exclusiva do grupo, criada pelo Organizador. Cada membro conclui uma vez
// (GrupoMissaoConclusao). Dá XP e moedas de grupo.
public class GrupoMissao
{
    public int Id { get; set; }

    public int GrupoId { get; set; }
    [JsonIgnore] public Grupo? Grupo { get; set; }

    public string Titulo { get; set; } = string.Empty;
    public int RecompensaXp { get; set; }
    public int RecompensaMoedas { get; set; }

    // Prazo opcional definido pelo Organizador — após ele, não dá mais para concluir.
    public DateTime? DataLimite { get; set; }

    [JsonIgnore] public ICollection<GrupoMissaoConclusao> Conclusoes { get; set; } = [];
}
