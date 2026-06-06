using DamMonitor.Domain.Entities;
using DamMonitor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Rebus.Handlers;

namespace DamMonitor.Host.Messaging;

public sealed class SensorReadingMessageHandler(
    DamMonitorDbContext dbContext,
    ILogger<SensorReadingMessageHandler> logger) : IHandleMessages<SensorReadingMessage>
{
    public async Task Handle(SensorReadingMessage message)
    {
        var sensor = await dbContext.Sensores
            .SingleOrDefaultAsync(sensor => sensor.CodigoIdentificador == message.SensorId);

        if (sensor is null)
        {
            logger.LogWarning("Leitura ignorada: sensor desconhecido {SensorId}.", message.SensorId);
            return;
        }

        var timestamp = DateTime.SpecifyKind(message.Timestamp.UtcDateTime, DateTimeKind.Unspecified);

        dbContext.Leituras.Add(new Leitura
        {
            SensorId = sensor.Id,
            Timestamp = timestamp,
            ValorLeitura = message.ValorLeitura
        });

        if (message.ValorLeitura > sensor.LimiteAlerta)
        {
            dbContext.Alertas.Add(new Alerta
            {
                SensorId = sensor.Id,
                Mensagem = BuildAlertMessage(sensor, message.ValorLeitura),
                Timestamp = timestamp,
                Status = "ATIVO"
            });
        }

        await dbContext.SaveChangesAsync();
    }

    private static string BuildAlertMessage(Sensor sensor, decimal valorLeitura)
    {
        var tipo = sensor.Tipo.Equals("nivel", StringComparison.OrdinalIgnoreCase)
            ? "Nível da água"
            : "Pressão do piezômetro";

        return $"{tipo} acima do limite configurado ({valorLeitura:0.##} > {sensor.LimiteAlerta:0.##})";
    }
}
