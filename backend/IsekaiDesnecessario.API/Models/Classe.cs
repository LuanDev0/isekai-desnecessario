using System.Text.Json.Serialization;

namespace IsekaiDesnecessario.API.Models;

public class Classe
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? NomeFeminino { get; set; }
    public string Emoji { get; set; } = string.Empty;
    public int AtributoId { get; set; }

    public Atributo? Atributo { get; set; }
    [JsonIgnore]
    public ICollection<Perfil> Perfis { get; set; } = [];
}
