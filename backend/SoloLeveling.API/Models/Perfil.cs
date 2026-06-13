using System.Text.Json.Serialization;

namespace SoloLeveling.API.Models;

public class Perfil
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int Xp { get; set; }
    public int Moedas { get; set; }
    public string Rank { get; set; } = "F";
    public string Titulo { get; set; } = "Iniciante";
    public int Nivel { get; set; } = 1;
    public int ProximoNivelXp { get; set; } = 100;
    public string? FotoUrl { get; set; }

    // ── Lootbox ──────────────────────────────────────
    public int XpHoje { get; set; } = 0;
    public DateTime? DataXpHoje { get; set; }
    public DateTime? UltimaLootbox { get; set; }

    // ── Desafio do dia ────────────────────────────────
    public DateTime? DesafioRecusadoEm  { get; set; }
    public DateTime? DesafioConcluídoEm { get; set; }

    [JsonIgnore]
    public ICollection<BomHabito> BonsHabitos { get; set; } = [];
    [JsonIgnore]
    public ICollection<MauHabito> MausHabitos { get; set; } = [];
    [JsonIgnore]
    public ICollection<Missao> Missoes { get; set; } = [];
    [JsonIgnore]
    public ICollection<Recompensa> Recompensas { get; set; } = [];
    [JsonIgnore]
    public ICollection<HistoricoXp> HistoricoXp { get; set; } = [];
    [JsonIgnore]
    public ICollection<ItemInventario> Inventario { get; set; } = [];
}
