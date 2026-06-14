using System.Text.Json.Serialization;

namespace IsekaiDesnecessario.API.Models;

public class Recompensa
{
    public int Id { get; set; }
    public int PerfilId { get; set; }
    [JsonIgnore] public Perfil? Perfil { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Emoji { get; set; } = "🎁";
    public int Preco { get; set; }
    public bool Ativa { get; set; } = true;

    // Requisito de atributo para resgatar (opcional)
    public int? AtributoId { get; set; }
    [JsonIgnore] public Atributo? Atributo { get; set; }
    public int PontosNecessarios { get; set; } = 0;
}
