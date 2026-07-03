using System.Text.Json.Serialization;

namespace IsekaiDesnecessario.API.Models;

// Resgate de uma recompensa do grupo por um membro (histórico de compras da loja do grupo).
public class GrupoRecompensaResgate
{
    public int Id { get; set; }

    public int GrupoRecompensaId { get; set; }
    [JsonIgnore] public GrupoRecompensa? GrupoRecompensa { get; set; }

    public int GrupoMembroId { get; set; }
    [JsonIgnore] public GrupoMembro? GrupoMembro { get; set; }

    public DateTime ResgatadaEm { get; set; } = DateTime.UtcNow;
}
