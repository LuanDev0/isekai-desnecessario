using System.Text.Json.Serialization;

namespace IsekaiDesnecessario.API.Models;

// Participação de um Perfil num Grupo + economia interna do membro.
// XpGrupo/MoedasGrupo são separados do XP/moedas do perfil — nunca se misturam.
public class GrupoMembro
{
    public int Id { get; set; }

    public int GrupoId { get; set; }
    [JsonIgnore] public Grupo? Grupo { get; set; }

    public int PerfilId { get; set; }
    [JsonIgnore] public Perfil? Perfil { get; set; }

    public int XpGrupo { get; set; }
    public int MoedasGrupo { get; set; }

    public DateTime EntrouEm { get; set; } = DateTime.UtcNow;
}
