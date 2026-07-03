using System.Text.Json.Serialization;

namespace IsekaiDesnecessario.API.Models;

// Grupo pago pelo Organizador (assinatura separada do VIP — billing ainda não integrado).
// O Organizador é a CONTA (Usuario) que paga e administra; membros entram com um Perfil.
// Economia (XP/moedas) é própria do grupo e vive em GrupoMembro — nunca vai para o perfil.
public class Grupo
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;

    public int OrganizadorUsuarioId { get; set; }
    [JsonIgnore] public Usuario? Organizador { get; set; }

    // Plano contratado (Starter/Standard/Pro/Max) — define MaxMembros. Ver PlanosGrupo.
    public string Plano { get; set; } = "Starter";
    public int MaxMembros { get; set; } = 5;

    // false = assinatura cancelada: membros perdem acesso até o Organizador reativar.
    public bool Ativo { get; set; } = true;

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    [JsonIgnore] public ICollection<GrupoMembro>     Membros     { get; set; } = [];
    [JsonIgnore] public ICollection<GrupoConvite>    Convites    { get; set; } = [];
    [JsonIgnore] public ICollection<GrupoHabito>     Habitos     { get; set; } = [];
    [JsonIgnore] public ICollection<GrupoMissao>     Missoes     { get; set; } = [];
    [JsonIgnore] public ICollection<GrupoRecompensa> Recompensas { get; set; } = [];
    [JsonIgnore] public ICollection<GrupoFeedEvento> Feed        { get; set; } = [];
}

// Planos disponíveis e capacidade de cada um. Upgrade só para plano maior.
public static class PlanosGrupo
{
    public static readonly IReadOnlyDictionary<string, int> Tamanhos = new Dictionary<string, int>
    {
        ["Starter"]  = 5,
        ["Standard"] = 10,
        ["Pro"]      = 30,
        ["Max"]      = 50,
    };
}
