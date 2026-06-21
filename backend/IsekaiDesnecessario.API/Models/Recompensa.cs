using System.Text.Json.Serialization;

namespace IsekaiDesnecessario.API.Models;

// Definição (molde) de uma recompensa — global (catálogo) ou própria (privada).
// A ativação no perfil (estar na loja do perfil) vive em PerfilRecompensa.
public class Recompensa
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Emoji { get; set; } = "🎁";
    public int Preco { get; set; }
    public bool Ativa { get; set; } = true;

    // Requisito de atributo para resgatar (opcional)
    public int? AtributoId { get; set; }
    [JsonIgnore] public Atributo? Atributo { get; set; }
    public int PontosNecessarios { get; set; } = 0;

    // ── Catálogo / autoria ───────────────────────────────
    public EscopoConteudo Escopo { get; set; } = EscopoConteudo.Global;
    public StatusConteudo Status { get; set; } = StatusConteudo.Aprovado;
    public int? CriadoPorUsuarioId { get; set; }
    [JsonIgnore] public Usuario? CriadoPor { get; set; }

    // Classes RPG às quais o item é exclusivo (vazio = vale para todas). Só conteúdo global usa.
    [JsonIgnore] public ICollection<Classe> Classes { get; set; } = [];

    [JsonIgnore] public ICollection<PerfilRecompensa> Ativacoes { get; set; } = [];
}
