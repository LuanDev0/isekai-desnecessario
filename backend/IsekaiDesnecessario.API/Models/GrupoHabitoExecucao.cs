using System.Text.Json.Serialization;

namespace IsekaiDesnecessario.API.Models;

// Registro de execução de um hábito do grupo por um membro.
// A frequência (Diário/Semanal/Mensal) é validada contra a última execução do membro.
public class GrupoHabitoExecucao
{
    public int Id { get; set; }

    public int GrupoHabitoId { get; set; }
    [JsonIgnore] public GrupoHabito? GrupoHabito { get; set; }

    public int GrupoMembroId { get; set; }
    [JsonIgnore] public GrupoMembro? GrupoMembro { get; set; }

    public DateTime ExecutadoEm { get; set; } = DateTime.UtcNow;
}
