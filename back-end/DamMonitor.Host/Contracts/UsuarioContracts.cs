using System.Text.Json.Serialization;

namespace DamMonitor.Host.Contracts;

public sealed record UsuarioRequest(
    [property: JsonPropertyName("nome")] string Nome,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("senha_hash")] string SenhaHash,
    [property: JsonPropertyName("papel")] string Role);

public sealed record UsuarioResponse(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("nome")] string Nome,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("papel")] string Role);
