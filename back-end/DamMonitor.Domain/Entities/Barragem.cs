namespace DamMonitor.Domain.Entities;

public sealed class Barragem
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public required string Localizacao { get; set; }
    public decimal NivelCriticoMetros { get; set; }
    public ICollection<Sensor> Sensores { get; set; } = [];
}
