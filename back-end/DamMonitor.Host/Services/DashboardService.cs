using DamMonitor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DamMonitor.Host.Services;

public sealed class DashboardService(DamMonitorDbContext dbContext)
{
    public async Task<object> GetAsync()
    {
        return new
        {
            barragens = await dbContext.Barragens.CountAsync(),
            sensores = await dbContext.Sensores.CountAsync(),
            alertas_ativos = await dbContext.Alertas.CountAsync(alerta => alerta.Status == "ATIVO"),
            ultimas_leituras = await dbContext.Leituras.AsNoTracking()
                .OrderByDescending(leitura => leitura.Timestamp)
                .Take(10)
                .Select(leitura => new
                {
                    id = leitura.Id,
                    timestamp = leitura.Timestamp,
                    valor_leitura = leitura.ValorLeitura,
                    sensor = leitura.Sensor == null ? null : new
                    {
                        codigo_identificador = leitura.Sensor.CodigoIdentificador,
                        tipo = leitura.Sensor.Tipo
                    }
                })
                .ToListAsync()
        };
    }
}
