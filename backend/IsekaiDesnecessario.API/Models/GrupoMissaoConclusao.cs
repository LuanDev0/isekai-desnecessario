using System.Text.Json.Serialization;

namespace IsekaiDesnecessario.API.Models;

// Conclusão de uma missão do grupo por um membro — cada membro conclui uma única vez.
public class GrupoMissaoConclusao
{
    public int Id { get; set; }

    public int GrupoMissaoId { get; set; }
    [JsonIgnore] public GrupoMissao? GrupoMissao { get; set; }

    public int GrupoMembroId { get; set; }
    [JsonIgnore] public GrupoMembro? GrupoMembro { get; set; }

    public DateTime ConcluidaEm { get; set; } = DateTime.UtcNow;
}
