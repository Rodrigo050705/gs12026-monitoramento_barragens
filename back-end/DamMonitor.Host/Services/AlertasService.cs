using DamMonitor.Domain.Entities;
using DamMonitor.Host.Contracts;
using DamMonitor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DamMonitor.Host.Services;

public sealed class AlertasService(DamMonitorDbContext dbContext)
{
    public async Task<IReadOnlyList<AlertaResponse>> GetAllAsync()
    {
        return await dbContext.Alertas.AsNoTracking()
            .OrderByDescending(alerta => alerta.Timestamp)
            .Select(alerta => new AlertaResponse(
                alerta.Id,
                alerta.Mensagem,
                alerta.Timestamp,
                alerta.Status,
                alerta.Sensor == null
                    ? null
                    : new AlertaSensorResponse(alerta.Sensor.Id, alerta.Sensor.CodigoIdentificador, alerta.Sensor.Tipo)))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<AlertaResponse>> GetActiveAsync()
    {
        return await dbContext.Alertas.AsNoTracking()
            .Where(alerta => alerta.Status == "ATIVO")
            .OrderByDescending(alerta => alerta.Timestamp)
            .Select(alerta => new AlertaResponse(
                alerta.Id,
                alerta.Mensagem,
                alerta.Timestamp,
                alerta.Status,
                alerta.Sensor == null
                    ? null
                    : new AlertaSensorResponse(alerta.Sensor.Id, alerta.Sensor.CodigoIdentificador, alerta.Sensor.Tipo)))
            .ToListAsync();
    }

    public async Task<AlertaResponse?> GetByIdAsync(int id)
    {
        return await dbContext.Alertas.AsNoTracking()
            .Where(alerta => alerta.Id == id)
            .Select(alerta => new AlertaResponse(
                alerta.Id,
                alerta.Mensagem,
                alerta.Timestamp,
                alerta.Status,
                alerta.Sensor == null
                    ? null
                    : new AlertaSensorResponse(alerta.Sensor.Id, alerta.Sensor.CodigoIdentificador, alerta.Sensor.Tipo)))
            .SingleOrDefaultAsync();
    }

    public async Task<ServiceResult<AlertaResponse>> CreateAsync(AlertaRequest request)
    {
        if (!await dbContext.Sensores.AnyAsync(sensor => sensor.Id == request.SensorId))
        {
            return ServiceResult<AlertaResponse>.BadRequest("Sensor não encontrado.");
        }

        var alerta = new Alerta
        {
            SensorId = request.SensorId,
            Mensagem = request.Mensagem,
            Timestamp = NormalizeTimestamp(request.Timestamp),
            Status = request.Status
        };

        dbContext.Alertas.Add(alerta);
        await dbContext.SaveChangesAsync();

        return ServiceResult<AlertaResponse>.Success((await GetByIdAsync(alerta.Id))!);
    }

    public async Task<ServiceResult<AlertaResponse>> UpdateAsync(int id, AlertaRequest request)
    {
        var alerta = await dbContext.Alertas.SingleOrDefaultAsync(alerta => alerta.Id == id);
        if (alerta is null)
        {
            return ServiceResult<AlertaResponse>.NotFound("Alerta não encontrado.");
        }

        if (!await dbContext.Sensores.AnyAsync(sensor => sensor.Id == request.SensorId))
        {
            return ServiceResult<AlertaResponse>.BadRequest("Sensor não encontrado.");
        }

        alerta.SensorId = request.SensorId;
        alerta.Mensagem = request.Mensagem;
        alerta.Timestamp = NormalizeTimestamp(request.Timestamp);
        alerta.Status = request.Status;

        await dbContext.SaveChangesAsync();

        return ServiceResult<AlertaResponse>.Success((await GetByIdAsync(alerta.Id))!);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var alerta = await dbContext.Alertas.SingleOrDefaultAsync(alerta => alerta.Id == id);
        if (alerta is null)
        {
            return ServiceResult<bool>.NotFound("Alerta não encontrado.");
        }

        dbContext.Alertas.Remove(alerta);
        await dbContext.SaveChangesAsync();

        return ServiceResult<bool>.Success(true);
    }
    private static DateTime NormalizeTimestamp(DateTime timestamp)
    {
        return DateTime.SpecifyKind(timestamp.ToUniversalTime(), DateTimeKind.Unspecified);
    }
}
