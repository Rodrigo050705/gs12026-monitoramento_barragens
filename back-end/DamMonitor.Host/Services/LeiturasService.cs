using DamMonitor.Domain.Entities;
using DamMonitor.Host.Contracts;
using DamMonitor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DamMonitor.Host.Services;

public sealed class LeiturasService(DamMonitorDbContext dbContext)
{
    public async Task<IReadOnlyList<LeituraResponse>> GetAllAsync(int take)
    {
        var limit = Math.Clamp(take, 1, 500);

        return await dbContext.Leituras.AsNoTracking()
            .OrderByDescending(leitura => leitura.Timestamp)
            .Take(limit)
            .Select(leitura => new LeituraResponse(leitura.Id, leitura.SensorId, leitura.Timestamp, leitura.ValorLeitura))
            .ToListAsync();
    }

    public async Task<LeituraResponse?> GetByIdAsync(long id)
    {
        return await dbContext.Leituras.AsNoTracking()
            .Where(leitura => leitura.Id == id)
            .Select(leitura => new LeituraResponse(leitura.Id, leitura.SensorId, leitura.Timestamp, leitura.ValorLeitura))
            .SingleOrDefaultAsync();
    }

    public async Task<ServiceResult<LeituraResponse>> CreateAsync(LeituraRequest request)
    {
        if (!await dbContext.Sensores.AnyAsync(sensor => sensor.Id == request.SensorId))
        {
            return ServiceResult<LeituraResponse>.BadRequest("Sensor não encontrado.");
        }

        var leitura = new Leitura
        {
            SensorId = request.SensorId,
            Timestamp = NormalizeTimestamp(request.Timestamp),
            ValorLeitura = request.ValorLeitura
        };

        dbContext.Leituras.Add(leitura);
        await dbContext.SaveChangesAsync();

        return ServiceResult<LeituraResponse>.Success(new LeituraResponse(
            leitura.Id,
            leitura.SensorId,
            leitura.Timestamp,
            leitura.ValorLeitura));
    }

    public async Task<ServiceResult<LeituraResponse>> UpdateAsync(long id, LeituraRequest request)
    {
        var leitura = await dbContext.Leituras.SingleOrDefaultAsync(leitura => leitura.Id == id);
        if (leitura is null)
        {
            return ServiceResult<LeituraResponse>.NotFound("Leitura não encontrada.");
        }

        if (!await dbContext.Sensores.AnyAsync(sensor => sensor.Id == request.SensorId))
        {
            return ServiceResult<LeituraResponse>.BadRequest("Sensor não encontrado.");
        }

        leitura.SensorId = request.SensorId;
        leitura.Timestamp = NormalizeTimestamp(request.Timestamp);
        leitura.ValorLeitura = request.ValorLeitura;

        await dbContext.SaveChangesAsync();

        return ServiceResult<LeituraResponse>.Success(new LeituraResponse(
            leitura.Id,
            leitura.SensorId,
            leitura.Timestamp,
            leitura.ValorLeitura));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(long id)
    {
        var leitura = await dbContext.Leituras.SingleOrDefaultAsync(leitura => leitura.Id == id);
        if (leitura is null)
        {
            return ServiceResult<bool>.NotFound("Leitura não encontrada.");
        }

        dbContext.Leituras.Remove(leitura);
        await dbContext.SaveChangesAsync();

        return ServiceResult<bool>.Success(true);
    }

    private static DateTime NormalizeTimestamp(DateTime timestamp)
    {
        return DateTime.SpecifyKind(timestamp.ToUniversalTime(), DateTimeKind.Unspecified);
    }
}
