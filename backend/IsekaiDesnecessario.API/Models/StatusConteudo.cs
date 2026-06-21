using System.Text.Json.Serialization;

namespace IsekaiDesnecessario.API.Models;

/// <summary>
/// Estado de aprovação de um item do catálogo global.
/// Itens próprios (privados) nascem já <see cref="Aprovado"/>.
/// Fluxo de aprovação Moderador → Admin é tratado na Parte 5.
/// Gravado como texto no banco.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StatusConteudo
{
    /// <summary>Visível no catálogo / utilizável.</summary>
    Aprovado = 0,
    /// <summary>Criado por Moderador, aguardando aprovação do Admin (não aparece no catálogo).</summary>
    Pendente = 1,
    /// <summary>Recusado pelo Admin.</summary>
    Rejeitado = 2,
}
