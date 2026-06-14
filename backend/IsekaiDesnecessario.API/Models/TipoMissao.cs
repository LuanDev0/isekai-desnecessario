using System.Text.Json.Serialization;

namespace IsekaiDesnecessario.API.Models;

public class TipoMissao
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;

    [JsonIgnore]
    public ICollection<Missao> Missoes { get; set; } = [];
}
