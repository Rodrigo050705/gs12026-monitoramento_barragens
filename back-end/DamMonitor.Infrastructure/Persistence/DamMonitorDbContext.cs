using DamMonitor.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DamMonitor.Infrastructure.Persistence;

public sealed class DamMonitorDbContext(DbContextOptions<DamMonitorDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Barragem> Barragens => Set<Barragem>();
    public DbSet<Sensor> Sensores => Set<Sensor>();
    public DbSet<Leitura> Leituras => Set<Leitura>();
    public DbSet<Alerta> Alertas => Set<Alerta>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("usuarios");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Nome).HasColumnName("nome").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(150).IsRequired();
            entity.Property(e => e.SenhaHash).HasColumnName("senha_hash").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Role).HasColumnName("role").HasMaxLength(20).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<Barragem>(entity =>
        {
            entity.ToTable("barragens");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Nome).HasColumnName("nome").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Localizacao).HasColumnName("localizacao").HasMaxLength(200).IsRequired();
            entity.Property(e => e.NivelCriticoMetros).HasColumnName("nivel_critico_metros").HasPrecision(10, 2).IsRequired();
        });

        modelBuilder.Entity<Sensor>(entity =>
        {
            entity.ToTable("sensores");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CodigoIdentificador).HasColumnName("codigo_identificador").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Tipo).HasColumnName("tipo").HasMaxLength(30).IsRequired();
            entity.Property(e => e.LimiteAlerta).HasColumnName("limite_alerta").HasPrecision(10, 2).IsRequired();
            entity.Property(e => e.BarragemId).HasColumnName("barragem_id");
            entity.HasIndex(e => e.CodigoIdentificador).IsUnique();
            entity.HasOne(e => e.Barragem)
                .WithMany(e => e.Sensores)
                .HasForeignKey(e => e.BarragemId)
                .HasConstraintName("fk_sensor_barragem");
        });

        modelBuilder.Entity<Leitura>(entity =>
        {
            entity.ToTable("leituras");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SensorId).HasColumnName("sensor_id");
            entity.Property(e => e.Timestamp).HasColumnName("timestamp").HasColumnType("timestamp").IsRequired();
            entity.Property(e => e.ValorLeitura).HasColumnName("valor_leitura").HasPrecision(10, 2).IsRequired();
            entity.HasIndex(e => new { e.SensorId, e.Timestamp });
            entity.HasOne(e => e.Sensor)
                .WithMany(e => e.Leituras)
                .HasForeignKey(e => e.SensorId)
                .HasConstraintName("fk_leitura_sensor");
        });

        modelBuilder.Entity<Alerta>(entity =>
        {
            entity.ToTable("alertas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SensorId).HasColumnName("sensor_id");
            entity.Property(e => e.Mensagem).HasColumnName("mensagem").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Timestamp).HasColumnName("timestamp").HasColumnType("timestamp").IsRequired();
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
            entity.HasIndex(e => e.Status);
            entity.HasOne(e => e.Sensor)
                .WithMany(e => e.Alertas)
                .HasForeignKey(e => e.SensorId)
                .HasConstraintName("fk_alerta_sensor");
        });
    }
}
