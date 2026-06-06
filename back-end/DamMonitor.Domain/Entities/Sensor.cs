namespace DamMonitor.Domain.Entities;

public sealed class Sensor
{
    public int Id { get; set; }
    public required string CodigoIdentificador { get; set; }
    public required string Tipo { get; set; }
    public decimal LimiteAlerta { get; set; }
    public int BarragemId { get; set; }
    public Barragem? Barragem { get; set; }
    public ICollection<Leitura> Leituras { get; set; } = [];
    public ICollection<Alerta> Alertas { get; set; } = [];
}
