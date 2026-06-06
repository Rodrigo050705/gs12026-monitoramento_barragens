using System.Text.Json.Serialization;

namespace DamMonitor.Host.Contracts;

public sealed record SensorRequest(
    [property: JsonPropertyName("codigo_identificador")] string CodigoIdentificador,
    [property: JsonPropertyName("tipo")] string Tipo,
    [property: JsonPropertyName("limite_alerta")] decimal LimiteAlerta,
    [property: JsonPropertyName("barragem_id")] int BarragemId);

public sealed record SensorBarragemResponse(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("nome")] string Nome,
    [property: JsonPropertyName("localizacao")] string Localizacao);

public sealed record SensorResponse(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("codigo_identificador")] string CodigoIdentificador,
    [property: JsonPropertyName("tipo")] string Tipo,
    [property: JsonPropertyName("limite_alerta")] decimal LimiteAlerta,
    [property: JsonPropertyName("barragem")] SensorBarragemResponse? Barragem);
