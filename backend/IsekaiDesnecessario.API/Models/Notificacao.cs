using System.Text.Json.Serialization;

namespace IsekaiDesnecessario.API.Models;

// Notificação in-app (sininho). Destinatário é a CONTA (Usuario), pois o papel é por conta.
// Tipos: "pendente" (admin: novo item p/ aprovar) · "rejeitado" / "modificado" (moderador autor).
public class Notificacao
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }
    [JsonIgnore] public Usuario? Usuario { get; set; }

    public string Tipo { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
    public bool Lida { get; set; }
    public DateTime CriadaEm { get; set; } = DateTime.UtcNow;
}
