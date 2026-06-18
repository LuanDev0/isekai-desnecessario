using System.Text.Json.Serialization;

namespace IsekaiDesnecessario.API.Models;

public class Perfil
{
    public int Id { get; set; }

    // ── Conta Google dona deste herói (null = perfil convidado) ──
    public int? UsuarioId { get; set; }
    [JsonIgnore]
    public Usuario? Usuario { get; set; }

    public string Nome { get; set; } = string.Empty;
    public string? Genero { get; set; }
    public int? ClasseId { get; set; }
    [JsonIgnore]
    public Classe? Classe { get; set; }
    public int Xp { get; set; }
    public int Moedas { get; set; }
    public string Rank { get; set; } = "H";
    public string Titulo { get; set; } = "Iniciante";
    public int Nivel { get; set; } = 1;
    public int ProximoNivelXp { get; set; } = 100;
    public string? FotoUrl { get; set; }

    // ── Lootbox ──────────────────────────────────────
    public int XpHoje { get; set; } = 0;
    public DateTime? DataXpHoje { get; set; }
    public DateTime? UltimaLootbox { get; set; }

    // Zera o XP acumulado do dia se virou um novo dia (UTC). Retorna true se resetou.
    public bool ResetarXpDiarioSeNovoDia()
    {
        if (DataXpHoje?.Date == DateTime.UtcNow.Date) return false;
        XpHoje     = 0;
        DataXpHoje = DateTime.UtcNow;
        return true;
    }

    // ── Desafio do dia ────────────────────────────────
    public DateTime? DesafioRecusadoEm  { get; set; }
    public DateTime? DesafioConcluidoEm { get; set; }

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
