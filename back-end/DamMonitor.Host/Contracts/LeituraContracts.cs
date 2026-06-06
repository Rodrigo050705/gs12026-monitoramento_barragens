using System.Text.Json.Serialization;

namespace DamMonitor.Host.Contracts;

public sealed record LeituraRequest(
    [property: JsonPropertyName("sensor_id")] int SensorId,
    [property: JsonPropertyName("timestamp")] DateTime Timestamp,
    [property: JsonPropertyName("valor_leitura")] decimal ValorLeitura);

public sealed record LeituraResponse(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("sensor_id")] int SensorId,
    [property: JsonPropertyName("timestamp")] DateTime Timestamp,
    [property: JsonPropertyName("valor_leitura")] decimal ValorLeitura);
