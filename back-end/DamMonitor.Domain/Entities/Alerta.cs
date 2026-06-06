namespace DamMonitor.Domain.Entities;

public sealed class Alerta
{
    public int Id { get; set; }
    public int SensorId { get; set; }
    public Sensor? Sensor { get; set; }
    public required string Mensagem { get; set; }
    public DateTime Timestamp { get; set; }
    public required string Status { get; set; }
}
