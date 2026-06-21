using System.Text.Json.Serialization;

namespace IsekaiDesnecessario.API.Models;

// Definição (molde) de uma missão — global (catálogo) ou própria (privada).
// O estado por-perfil (concluída, quando, streak, ativo) vive em PerfilMissao.
public class Missao
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public int TipoId { get; set; }
    public TipoMissao? Tipo { get; set; }
    public int RecompensaXp { get; set; }
    public int RecompensaMoedas { get; set; }
    public DateTime? DataLimite { get; set; }
    // Vincula uma secundária a uma principal (id de outra definição de missão).
    public int? MissaoPrincipalId { get; set; }
    public int? AtributoId { get; set; }
    public Atributo? Atributo { get; set; }

    // ── Catálogo / autoria ───────────────────────────────
    public EscopoConteudo Escopo { get; set; } = EscopoConteudo.Global;
    public StatusConteudo Status { get; set; } = StatusConteudo.Aprovado;
    public int? CriadoPorUsuarioId { get; set; }
    [JsonIgnore] public Usuario? CriadoPor { get; set; }

    // Classes RPG às quais o item é exclusivo (vazio = vale para todas). Só conteúdo global usa.
    [JsonIgnore] public ICollection<Classe> Classes { get; set; } = [];

    [JsonIgnore] public ICollection<PerfilMissao> Ativacoes { get; set; } = [];
}
