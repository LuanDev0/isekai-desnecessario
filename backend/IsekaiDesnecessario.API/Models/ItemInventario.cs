using System.Text.Json.Serialization;

namespace IsekaiDesnecessario.API.Models;

public class ItemInventario
{
    public int Id { get; set; }
    public int PerfilId { get; set; }

    [JsonIgnore]
    public Perfil? Perfil { get; set; }

    // Dados desnormalizados — preserva info mesmo se recompensa for deletada
    public int RecompensaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Emoji { get; set; } = "🎁";
    public string Descricao { get; set; } = string.Empty;
    public int Preco { get; set; }

    public DateTime DataCompra { get; set; } = DateTime.Now;
    public DateTime? DataUso { get; set; }
    public bool Usado { get; set; } = false;
}
