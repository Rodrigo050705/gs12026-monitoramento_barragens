using System.Text.Json.Serialization;

namespace DamMonitor.Host.Contracts;

public sealed record LoginRequest(
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("senha")] string Senha);

public sealed record LoginUsuarioResponse(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("nome")] string Nome,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("papel")] string Role);

public sealed record LoginResponse(
    [property: JsonPropertyName("token_acesso")] string AccessToken,
    [property: JsonPropertyName("tipo_token")] string TokenType,
    [property: JsonPropertyName("expira_em")] DateTime ExpiresAt,
    [property: JsonPropertyName("usuario")] LoginUsuarioResponse Usuario);
