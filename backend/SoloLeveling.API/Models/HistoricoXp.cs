using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SoloLeveling.API.Models;

public class HistoricoXp
{
    public int Id { get; set; }
    public int PerfilId { get; set; }

    [JsonIgnore]
    public Perfil? Perfil { get; set; }

    public DateOnly Data { get; set; }
    public int XpHoje { get; set; }
    public int Nivel { get; set; }
    public int Moedas { get; set; }

    // Armazena array de 24 ints como JSON no banco
    public string XpPorHoraJson { get; set; } = JsonSerializer.Serialize(new int[24]);

    [NotMapped]
    public int[] XpPorHora
    {
        get
        {
            var arr = JsonSerializer.Deserialize<int[]>(XpPorHoraJson);
            if (arr is null || arr.Length < 24)
            {
                var full = new int[24];
                if (arr != null) Array.Copy(arr, full, Math.Min(arr.Length, 24));
                return full;
            }
            return arr;
        }
        set => XpPorHoraJson = JsonSerializer.Serialize(value);
    }
}
