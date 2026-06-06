using System.Text.Json.Serialization;

namespace DamMonitor.Host.Contracts;

public sealed record AlertaRequest(
    [property: JsonPropertyName("sensor_id")] int SensorId,
    [property: JsonPropertyName("mensagem")] string Mensagem,
    [property: JsonPropertyName("timestamp")] DateTime Timestamp,
    [property: JsonPropertyName("status")] string Status);

public sealed record AlertaSensorResponse(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("codigo_identificador")] string CodigoIdentificador,
    [property: JsonPropertyName("tipo")] string Tipo);

public sealed record AlertaResponse(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("mensagem")] string Mensagem,
    [property: JsonPropertyName("timestamp")] DateTime Timestamp,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("sensor")] AlertaSensorResponse? Sensor);
