using System.Text.Json.Serialization;

namespace IsekaiDesnecessario.API.Models;

public class SnapshotAtributo
{
    public int      Id         { get; set; }
    public int      PerfilId   { get; set; }
    [JsonIgnore] public Perfil? Perfil { get; set; }
    public int      AtributoId { get; set; }
    [JsonIgnore] public Atributo? Atributo { get; set; }
    public int      Pontos     { get; set; }
    public DateTime Data       { get; set; } = DateTime.UtcNow.Date;
}
