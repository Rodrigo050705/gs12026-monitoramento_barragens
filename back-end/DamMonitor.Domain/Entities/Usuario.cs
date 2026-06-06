namespace DamMonitor.Domain.Entities;

public sealed class Usuario
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public required string Email { get; set; }
    public required string SenhaHash { get; set; }
    public required string Role { get; set; }
}
