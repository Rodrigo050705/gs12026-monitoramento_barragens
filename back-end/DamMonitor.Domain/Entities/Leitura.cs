namespace DamMonitor.Domain.Entities;

public sealed class Leitura
{
    public long Id { get; set; }
    public int SensorId { get; set; }
    public Sensor? Sensor { get; set; }
    public DateTime Timestamp { get; set; }
    public decimal ValorLeitura { get; set; }
}
