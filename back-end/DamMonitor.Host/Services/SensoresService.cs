using DamMonitor.Domain.Entities;
using DamMonitor.Host.Contracts;
using DamMonitor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DamMonitor.Host.Services;

public sealed class SensoresService(DamMonitorDbContext dbContext)
{
    public async Task<IReadOnlyList<SensorResponse>> GetAllAsync()
    {
        return await dbContext.Sensores.AsNoTracking()
            .OrderBy(sensor => sensor.CodigoIdentificador)
            .Select(sensor => new SensorResponse(
                sensor.Id,
                sensor.CodigoIdentificador,
                sensor.Tipo,
                sensor.LimiteAlerta,
                sensor.Barragem == null
                    ? null
                    : new SensorBarragemResponse(sensor.Barragem.Id, sensor.Barragem.Nome, sensor.Barragem.Localizacao)))
            .ToListAsync();
    }

    public async Task<SensorResponse?> GetByIdAsync(int id)
    {
        return await dbContext.Sensores.AsNoTracking()
            .Where(sensor => sensor.Id == id)
            .Select(sensor => new SensorResponse(
                sensor.Id,
                sensor.CodigoIdentificador,
                sensor.Tipo,
                sensor.LimiteAlerta,
                sensor.Barragem == null
                    ? null
                    : new SensorBarragemResponse(sensor.Barragem.Id, sensor.Barragem.Nome, sensor.Barragem.Localizacao)))
            .SingleOrDefaultAsync();
    }

    public async Task<IReadOnlyList<LeituraResponse>> GetReadingsAsync(string codigoIdentificador, int take)
    {
        var limit = Math.Clamp(take, 1, 500);

        return await dbContext.Leituras.AsNoTracking()
            .Where(leitura => leitura.Sensor != null && leitura.Sensor.CodigoIdentificador == codigoIdentificador)
            .OrderByDescending(leitura => leitura.Timestamp)
            .Take(limit)
            .Select(leitura => new LeituraResponse(leitura.Id, leitura.SensorId, leitura.Timestamp, leitura.ValorLeitura))
            .ToListAsync();
    }

    public async Task<ServiceResult<SensorResponse>> CreateAsync(SensorRequest request)
    {
        if (!await dbContext.Barragens.AnyAsync(barragem => barragem.Id == request.BarragemId))
        {
            return ServiceResult<SensorResponse>.BadRequest("Barragem não encontrada.");
        }

        if (await dbContext.Sensores.AnyAsync(sensor => sensor.CodigoIdentificador == request.CodigoIdentificador))
        {
            return ServiceResult<SensorResponse>.Conflict("Já existe um sensor com este identificador.");
        }

        var sensor = new Sensor
        {
            CodigoIdentificador = request.CodigoIdentificador,
            Tipo = request.Tipo,
            LimiteAlerta = request.LimiteAlerta,
            BarragemId = request.BarragemId
        };

        dbContext.Sensores.Add(sensor);
        await dbContext.SaveChangesAsync();

        return ServiceResult<SensorResponse>.Success((await GetByIdAsync(sensor.Id))!);
    }

    public async Task<ServiceResult<SensorResponse>> UpdateAsync(int id, SensorRequest request)
    {
        var sensor = await dbContext.Sensores.SingleOrDefaultAsync(sensor => sensor.Id == id);
        if (sensor is null)
        {
            return ServiceResult<SensorResponse>.NotFound("Sensor não encontrado.");
        }

        if (!await dbContext.Barragens.AnyAsync(barragem => barragem.Id == request.BarragemId))
        {
            return ServiceResult<SensorResponse>.BadRequest("Barragem não encontrada.");
        }

        if (await dbContext.Sensores.AnyAsync(existing => existing.Id != id && existing.CodigoIdentificador == request.CodigoIdentificador))
        {
            return ServiceResult<SensorResponse>.Conflict("Já existe um sensor com este identificador.");
        }

        sensor.CodigoIdentificador = request.CodigoIdentificador;
        sensor.Tipo = request.Tipo;
        sensor.LimiteAlerta = request.LimiteAlerta;
        sensor.BarragemId = request.BarragemId;

        await dbContext.SaveChangesAsync();

        return ServiceResult<SensorResponse>.Success((await GetByIdAsync(sensor.Id))!);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var sensor = await dbContext.Sensores.SingleOrDefaultAsync(sensor => sensor.Id == id);
        if (sensor is null)
        {
            return ServiceResult<bool>.NotFound("Sensor não encontrado.");
        }

        if (await dbContext.Leituras.AnyAsync(leitura => leitura.SensorId == id)
            || await dbContext.Alertas.AnyAsync(alerta => alerta.SensorId == id))
        {
            return ServiceResult<bool>.Conflict("O sensor possui leituras ou alertas e não pode ser excluído.");
        }

        dbContext.Sensores.Remove(sensor);
        await dbContext.SaveChangesAsync();

        return ServiceResult<bool>.Success(true);
    }
}
