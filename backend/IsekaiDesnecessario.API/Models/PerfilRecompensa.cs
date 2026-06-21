using System.Text.Json.Serialization;

namespace IsekaiDesnecessario.API.Models;

// Ativação de uma recompensa (definição) num perfil — está na loja deste perfil.
public class PerfilRecompensa
{
    public int Id { get; set; }

    public int PerfilId { get; set; }
    [JsonIgnore] public Perfil? Perfil { get; set; }

    public int RecompensaId { get; set; }
    public Recompensa? Recompensa { get; set; }

    public bool Ativo { get; set; } = true;

    // Trava por timer: enquanto > agora, não pode desativar (enforcement na Parte 5).
    public DateTime? TravadoAte { get; set; }
}
