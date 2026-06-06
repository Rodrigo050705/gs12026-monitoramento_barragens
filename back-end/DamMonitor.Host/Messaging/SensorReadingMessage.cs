namespace DamMonitor.Host.Messaging;

public sealed record SensorReadingMessage(
    string SensorId,
    DateTimeOffset Timestamp,
    string Tipo,
    decimal ValorLeitura,
    int StatusBateria);
