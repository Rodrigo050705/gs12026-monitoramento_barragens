using DamMonitor.Domain.Entities;
using DamMonitor.Host.Contracts;
using DamMonitor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DamMonitor.Host.Services;

public sealed class BarragensService(DamMonitorDbContext dbContext)
{
    public async Task<IReadOnlyList<BarragemResponse>> GetAllAsync()
    {
        return await dbContext.Barragens.AsNoTracking()
            .OrderBy(barragem => barragem.Nome)
            .Select(barragem => new BarragemResponse(
                barragem.Id,
                barragem.Nome,
                barragem.Localizacao,
                barragem.NivelCriticoMetros,
                barragem.Sensores.Count))
            .ToListAsync();
    }

    public async Task<BarragemResponse?> GetByIdAsync(int id)
    {
        return await dbContext.Barragens.AsNoTracking()
            .Where(barragem => barragem.Id == id)
            .Select(barragem => new BarragemResponse(
                barragem.Id,
                barragem.Nome,
                barragem.Localizacao,
                barragem.NivelCriticoMetros,
                barragem.Sensores.Count))
            .SingleOrDefaultAsync();
    }

    public async Task<ServiceResult<BarragemResponse>> CreateAsync(BarragemRequest request)
    {
        var barragem = new Barragem
        {
            Nome = request.Nome,
            Localizacao = request.Localizacao,
            NivelCriticoMetros = request.NivelCriticoMetros
        };

        dbContext.Barragens.Add(barragem);
        await dbContext.SaveChangesAsync();

        return ServiceResult<BarragemResponse>.Success(new BarragemResponse(
            barragem.Id,
            barragem.Nome,
            barragem.Localizacao,
            barragem.NivelCriticoMetros,
            0));
    }

    public async Task<ServiceResult<BarragemResponse>> UpdateAsync(int id, BarragemRequest request)
    {
        var barragem = await dbContext.Barragens.SingleOrDefaultAsync(barragem => barragem.Id == id);
        if (barragem is null)
        {
            return ServiceResult<BarragemResponse>.NotFound("Barragem não encontrada.");
        }

        barragem.Nome = request.Nome;
        barragem.Localizacao = request.Localizacao;
        barragem.NivelCriticoMetros = request.NivelCriticoMetros;

        await dbContext.SaveChangesAsync();

        var quantidadeSensores = await dbContext.Sensores.CountAsync(sensor => sensor.BarragemId == id);
        return ServiceResult<BarragemResponse>.Success(new BarragemResponse(
            barragem.Id,
            barragem.Nome,
            barragem.Localizacao,
            barragem.NivelCriticoMetros,
            quantidadeSensores));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var barragem = await dbContext.Barragens.SingleOrDefaultAsync(barragem => barragem.Id == id);
        if (barragem is null)
        {
            return ServiceResult<bool>.NotFound("Barragem não encontrada.");
        }

        if (await dbContext.Sensores.AnyAsync(sensor => sensor.BarragemId == id))
        {
            return ServiceResult<bool>.Conflict("A barragem possui sensores e não pode ser excluída.");
        }

        dbContext.Barragens.Remove(barragem);
        await dbContext.SaveChangesAsync();

        return ServiceResult<bool>.Success(true);
    }
}
