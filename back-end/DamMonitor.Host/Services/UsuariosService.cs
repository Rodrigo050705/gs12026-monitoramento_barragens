using DamMonitor.Domain.Entities;
using DamMonitor.Host.Contracts;
using DamMonitor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DamMonitor.Host.Services;

public sealed class UsuariosService(DamMonitorDbContext dbContext)
{
    public async Task<IReadOnlyList<UsuarioResponse>> GetAllAsync()
    {
        return await dbContext.Usuarios.AsNoTracking()
            .OrderBy(usuario => usuario.Nome)
            .Select(usuario => new UsuarioResponse(usuario.Id, usuario.Nome, usuario.Email, usuario.Role))
            .ToListAsync();
    }

    public async Task<UsuarioResponse?> GetByIdAsync(int id)
    {
        return await dbContext.Usuarios.AsNoTracking()
            .Where(usuario => usuario.Id == id)
            .Select(usuario => new UsuarioResponse(usuario.Id, usuario.Nome, usuario.Email, usuario.Role))
            .SingleOrDefaultAsync();
    }

    public async Task<ServiceResult<UsuarioResponse>> CreateAsync(UsuarioRequest request)
    {
        if (await dbContext.Usuarios.AnyAsync(usuario => usuario.Email == request.Email))
        {
            return ServiceResult<UsuarioResponse>.Conflict("Já existe um usuário com este email.");
        }

        var usuario = new Usuario
        {
            Nome = request.Nome,
            Email = request.Email,
            SenhaHash = request.SenhaHash,
            Role = request.Role
        };

        dbContext.Usuarios.Add(usuario);
        await dbContext.SaveChangesAsync();

        return ServiceResult<UsuarioResponse>.Success(new UsuarioResponse(usuario.Id, usuario.Nome, usuario.Email, usuario.Role));
    }

    public async Task<ServiceResult<UsuarioResponse>> UpdateAsync(int id, UsuarioRequest request)
    {
        var usuario = await dbContext.Usuarios.SingleOrDefaultAsync(usuario => usuario.Id == id);
        if (usuario is null)
        {
            return ServiceResult<UsuarioResponse>.NotFound("Usuário não encontrado.");
        }

        if (await dbContext.Usuarios.AnyAsync(existing => existing.Id != id && existing.Email == request.Email))
        {
            return ServiceResult<UsuarioResponse>.Conflict("Já existe um usuário com este email.");
        }

        usuario.Nome = request.Nome;
        usuario.Email = request.Email;
        usuario.SenhaHash = request.SenhaHash;
        usuario.Role = request.Role;

        await dbContext.SaveChangesAsync();

        return ServiceResult<UsuarioResponse>.Success(new UsuarioResponse(usuario.Id, usuario.Nome, usuario.Email, usuario.Role));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var usuario = await dbContext.Usuarios.SingleOrDefaultAsync(usuario => usuario.Id == id);
        if (usuario is null)
        {
            return ServiceResult<bool>.NotFound("Usuário não encontrado.");
        }

        dbContext.Usuarios.Remove(usuario);
        await dbContext.SaveChangesAsync();

        return ServiceResult<bool>.Success(true);
    }
}
