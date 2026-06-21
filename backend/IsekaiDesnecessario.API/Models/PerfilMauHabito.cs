using System.Text.Json.Serialization;

namespace IsekaiDesnecessario.API.Models;

// Ativação de um mau hábito (definição) num perfil + estado por-perfil.
public class PerfilMauHabito
{
    public int Id { get; set; }

    public int PerfilId { get; set; }
    [JsonIgnore] public Perfil? Perfil { get; set; }

    public int MauHabitoId { get; set; }
    public MauHabito? MauHabito { get; set; }

    public bool Ativo { get; set; } = true;
    public int Streak { get; set; }
    public DateTime? UltimaExecucao { get; set; }

    // Trava por timer: enquanto > agora, não pode desativar (enforcement na Parte 5).
    public DateTime? TravadoAte { get; set; }
}
