using System.Text.Json.Serialization;

namespace IsekaiDesnecessario.API.Models;

// Ativação de uma missão (definição) num perfil + estado por-perfil.
public class PerfilMissao
{
    public int Id { get; set; }

    public int PerfilId { get; set; }
    [JsonIgnore] public Perfil? Perfil { get; set; }

    public int MissaoId { get; set; }
    public Missao? Missao { get; set; }

    public bool Ativo { get; set; } = true;
    public bool Concluida { get; set; }
    public DateTime? ConcluidaEm { get; set; }
    public int Streak { get; set; }

    // Trava por timer: enquanto > agora, não pode desativar (enforcement na Parte 5).
    public DateTime? TravadoAte { get; set; }
}
