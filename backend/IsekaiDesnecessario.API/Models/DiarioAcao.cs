using System.Text.Json.Serialization;

namespace IsekaiDesnecessario.API.Models;

public class DiarioAcao
{
    public int Id { get; set; }
    public int PerfilId { get; set; }
    [JsonIgnore] public Perfil? Perfil { get; set; }

    public string Mensagem { get; set; } = string.Empty;
    public string Emoji    { get; set; } = "📝";
    // habito_bom | habito_mau | missao | nivel | recompensa | lootbox
    public string Tipo     { get; set; } = "acao";
    public DateTime Data   { get; set; } = DateTime.Now;
}
