using System.Text.Json.Serialization;

namespace DamMonitor.Host.Contracts;

public sealed record BarragemRequest(
    [property: JsonPropertyName("nome")] string Nome,
    [property: JsonPropertyName("localizacao")] string Localizacao,
    [property: JsonPropertyName("nivel_critico_metros")] decimal NivelCriticoMetros);

public sealed record BarragemResponse(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("nome")] string Nome,
    [property: JsonPropertyName("localizacao")] string Localizacao,
    [property: JsonPropertyName("nivel_critico_metros")] decimal NivelCriticoMetros,
    [property: JsonPropertyName("quantidade_sensores")] int QuantidadeSensores);
