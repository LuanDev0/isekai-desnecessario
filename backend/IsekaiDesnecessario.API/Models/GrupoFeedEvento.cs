using System.Text.Json.Serialization;

namespace IsekaiDesnecessario.API.Models;

// Evento da timeline do grupo ("diário de conquistas do grupo") — mensagem denormalizada.
// Tipos: entrou · saiu · removido · habito · missao · recompensa · conteudo · grupo.
public class GrupoFeedEvento
{
    public int Id { get; set; }

    public int GrupoId { get; set; }
    [JsonIgnore] public Grupo? Grupo { get; set; }

    public string Tipo { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
