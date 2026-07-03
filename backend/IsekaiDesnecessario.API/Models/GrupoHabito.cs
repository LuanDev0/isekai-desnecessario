using System.Text.Json.Serialization;

namespace IsekaiDesnecessario.API.Models;

// Hábito exclusivo do grupo, criado pelo Organizador. Vale para todos os membros;
// cada execução por membro vive em GrupoHabitoExecucao. Dá XP de grupo (sem moedas).
public class GrupoHabito
{
    public int Id { get; set; }

    public int GrupoId { get; set; }
    [JsonIgnore] public Grupo? Grupo { get; set; }

    public string Habito { get; set; } = string.Empty;
    public int Xp { get; set; }

    // Diário · Semanal · Mensal · Livre (mesma régua dos hábitos do jogo principal)
    public string Frequencia { get; set; } = "Diário";

    [JsonIgnore] public ICollection<GrupoHabitoExecucao> Execucoes { get; set; } = [];
}
