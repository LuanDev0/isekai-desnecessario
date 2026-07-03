using System.Text.Json.Serialization;

namespace IsekaiDesnecessario.API.Models;

// Convite enviado pelo Organizador para uma CONTA (por e-mail). Ao aceitar,
// o destinatário escolhe com qual Perfil entra (o app usa o perfil ativo).
public class GrupoConvite
{
    public int Id { get; set; }

    public int GrupoId { get; set; }
    [JsonIgnore] public Grupo? Grupo { get; set; }

    public int UsuarioId { get; set; }
    [JsonIgnore] public Usuario? Usuario { get; set; }

    // Pendente · Aceito · Recusado
    public string Status { get; set; } = "Pendente";

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
