namespace IsekaiDesnecessario.API.Models;

// Contrato comum das definições de conteúdo do catálogo (hábito/missão/recompensa).
// Usado pelo fluxo de aprovação para tratar os 4 tipos de forma genérica.
public interface IDefinicaoConteudo
{
    EscopoConteudo Escopo { get; set; }
    StatusConteudo Status { get; set; }
    int? CriadoPorUsuarioId { get; set; }
}
