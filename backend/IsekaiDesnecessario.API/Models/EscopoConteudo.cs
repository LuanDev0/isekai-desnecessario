using System.Text.Json.Serialization;

namespace IsekaiDesnecessario.API.Models;

/// <summary>
/// Onde vive a definição de um item de conteúdo (hábito/missão/recompensa).
/// Gravado como texto no banco para facilitar leitura/ajuste manual.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EscopoConteudo
{
    /// <summary>Catálogo compartilhado entre todos (criado por Admin/Moderador).</summary>
    Global = 0,
    /// <summary>Espaço privado de uma conta (criado por VIP/Admin/Moderador, só pra ela).</summary>
    Proprio = 1,
}
