using System.Text.Json.Serialization;

namespace IsekaiDesnecessario.API.Models;

public class Usuario
{
    public int Id { get; set; }

    // ── Identidade Google (opcional) ──────────────────
    public string? GoogleId { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool EmailVerificado { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? FotoUrl { get; set; }

    // ── Login próprio (opcional) ──────────────────────
    [JsonIgnore]
    public string? SenhaHash { get; set; }

    // ── Papel de acesso (vale para todos os perfis da conta) ──
    // UsuarioId = 1 é forçado a Admin no login/registro (hardcoded por enquanto);
    // demais papéis são ajustados manualmente no banco.
    public Role Role { get; set; } = Role.Usuario;

    // ── Auditoria ─────────────────────────────────────
    public DateTime CriadoEm    { get; set; } = DateTime.UtcNow;
    public DateTime UltimoLogin { get; set; } = DateTime.UtcNow;

    // ── Heróis (perfis) pertencentes a esta conta ─────
    [JsonIgnore]
    public ICollection<Perfil> Perfis { get; set; } = [];
}
