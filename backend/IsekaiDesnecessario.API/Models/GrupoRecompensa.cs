using System.Text.Json.Serialization;

namespace IsekaiDesnecessario.API.Models;

// Recompensa exclusiva do grupo, criada pelo Organizador.
// Comprada com moedas de grupo (GrupoMembro.MoedasGrupo); resgates em GrupoRecompensaResgate.
public class GrupoRecompensa
{
    public int Id { get; set; }

    public int GrupoId { get; set; }
    [JsonIgnore] public Grupo? Grupo { get; set; }

    public string Nome { get; set; } = string.Empty;
    public int Custo { get; set; }

    [JsonIgnore] public ICollection<GrupoRecompensaResgate> Resgates { get; set; } = [];
}
