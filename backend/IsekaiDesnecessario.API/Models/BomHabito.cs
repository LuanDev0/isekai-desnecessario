using System.Text.Json.Serialization;

namespace IsekaiDesnecessario.API.Models;

// Definição (molde) de um bom hábito — global (catálogo) ou próprio (privado).
// O estado por-perfil (streak, última execução, ativo) vive em PerfilBomHabito.
public class BomHabito : IDefinicaoConteudo
{
    public int Id { get; set; }
    public string Habito { get; set; } = string.Empty;
    public int Xp { get; set; }
    public string Frequencia { get; set; } = string.Empty;
    public int? AtributoId { get; set; }
    public Atributo? Atributo { get; set; }

    // Trava por timer (dias): ao concluir, a ativação fica travada por N dias e não
    // pode ser desativada. 0 = sem trava. Definido pelo criador do item.
    public int TravaDias { get; set; }

    // ── Catálogo / autoria ───────────────────────────────
    public EscopoConteudo Escopo { get; set; } = EscopoConteudo.Global;
    public StatusConteudo Status { get; set; } = StatusConteudo.Aprovado;
    public int? CriadoPorUsuarioId { get; set; }
    [JsonIgnore] public Usuario? CriadoPor { get; set; }

    // Classes RPG às quais o item é exclusivo (vazio = vale para todas). Só conteúdo global usa.
    [JsonIgnore] public ICollection<Classe> Classes { get; set; } = [];

    [JsonIgnore] public ICollection<PerfilBomHabito> Ativacoes { get; set; } = [];
}
