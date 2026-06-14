using System.Text.Json.Serialization;

namespace IsekaiDesnecessario.API.Models;

public class Usuario
{
    public int Id { get; set; }

    // ── Identidade Google ─────────────────────────────
    public string GoogleId { get; set; } = string.Empty;   // 'sub' do token Google (chave única)
    public string Email { get; set; } = string.Empty;
    public bool EmailVerificado { get; set; }
    public string Nome { get; set; } = string.Empty;        // nome da conta Google
    public string? FotoUrl { get; set; }                    // avatar do Google

    // ── Auditoria ─────────────────────────────────────
    public DateTime CriadoEm    { get; set; } = DateTime.Now;
    public DateTime UltimoLogin { get; set; } = DateTime.Now;

    // ── Heróis (perfis) pertencentes a esta conta ─────
    [JsonIgnore]
    public ICollection<Perfil> Perfis { get; set; } = [];
}
