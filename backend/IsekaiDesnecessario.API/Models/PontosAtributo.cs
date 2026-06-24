using System.Text.Json.Serialization;

namespace IsekaiDesnecessario.API.Models;

public class PontosAtributo
{
    public int PerfilId   { get; set; }
    [JsonIgnore] public Perfil?   Perfil   { get; set; }
    public int AtributoId { get; set; }
    [JsonIgnore] public Atributo? Atributo { get; set; }
    public int Total      { get; set; }
}
