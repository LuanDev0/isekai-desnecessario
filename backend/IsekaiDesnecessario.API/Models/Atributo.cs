using System.Text.Json.Serialization;

namespace IsekaiDesnecessario.API.Models;

public class Atributo
{
    public int    Id      { get; set; }
    public string Nome    { get; set; } = string.Empty;
    public string Emoji   { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Cor     { get; set; } = "#58a6ff";

    [JsonIgnore]
    public ICollection<BomHabito> BonsHabitos { get; set; } = [];
    [JsonIgnore]
    public ICollection<MauHabito> MausHabitos { get; set; } = [];
    [JsonIgnore]
    public ICollection<Missao> Missoes { get; set; } = [];
}
