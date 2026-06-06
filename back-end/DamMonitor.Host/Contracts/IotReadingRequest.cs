using System.Text.Json.Serialization;
using DamMonitor.Host.Messaging;

namespace DamMonitor.Host.Contracts;

public sealed record IotReadingRequest(
    [property: JsonPropertyName("sensor_id")] string SensorId,
    [property: JsonPropertyName("timestamp")] DateTimeOffset Timestamp,
    [property: JsonPropertyName("tipo")] string Tipo,
    [property: JsonPropertyName("leitura")] Dictionary<string, decimal> Leitura,
    [property: JsonPropertyName("status_bateria")] int StatusBateria)
{
    public bool TryToMessage(out SensorReadingMessage message, out string? error)
    {
        message = default!;

        if (string.IsNullOrWhiteSpace(SensorId))
        {
            error = "sensor_id é obrigatório.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Tipo))
        {
            error = "tipo é obrigatório.";
            return false;
        }

        if (Leitura is null || !TryGetValorLeitura(out var valorLeitura))
        {
            error = "leitura deve incluir pressao_kPa, altura_metros ou ao menos um valor numérico.";
            return false;
        }

        message = new SensorReadingMessage(SensorId, Timestamp, Tipo, valorLeitura, StatusBateria);
        error = null;
        return true;
    }

    private bool TryGetValorLeitura(out decimal valorLeitura)
    {
        if (Leitura.TryGetValue("pressao_kPa", out valorLeitura))
        {
            return true;
        }

        if (Leitura.TryGetValue("altura_metros", out valorLeitura))
        {
            return true;
        }

        if (Leitura.Count == 0)
        {
            valorLeitura = default;
            return false;
        }

        valorLeitura = Leitura.Values.First();
        return true;
    }
}
