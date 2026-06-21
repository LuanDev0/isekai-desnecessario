using System.Text.Json.Serialization;

namespace IsekaiDesnecessario.API.Models;

/// <summary>
/// Papel de acesso da CONTA (Usuario) — vale para todos os perfis dela.
/// Não confundir com Classe RPG (Mago, Bardo, etc.), que é do Perfil.
/// Armazenado como texto no banco para facilitar ajuste manual.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Role
{
    /// <summary>Apenas ativa/desativa itens do catálogo. Sem criação.</summary>
    Usuario = 0,
    /// <summary>Cria conteúdo próprio/privado. Sem acesso ao catálogo global.</summary>
    VIP = 1,
    /// <summary>Cria/edita conteúdo global (entra como pendente) + conteúdo próprio.</summary>
    Moderador = 2,
    /// <summary>Poder total: publica/edita/exclui no catálogo global + conteúdo próprio.</summary>
    Admin = 3,
}
