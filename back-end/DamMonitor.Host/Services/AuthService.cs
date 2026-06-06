using DamMonitor.Host.Contracts;
using DamMonitor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DamMonitor.Host.Services;

public sealed class AuthService(DamMonitorDbContext dbContext, JwtTokenService jwtTokenService)
{
    public async Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var usuario = await dbContext.Usuarios.AsNoTracking()
            .SingleOrDefaultAsync(usuario => usuario.Email == request.Email);

        if (usuario is null || usuario.SenhaHash != request.Senha)
        {
            return ServiceResult<LoginResponse>.NotFound("Email ou senha inválidos.");
        }

        var expiresAt = DateTime.UtcNow.AddHours(2);

        return ServiceResult<LoginResponse>.Success(new LoginResponse(
            jwtTokenService.Generate(usuario, expiresAt),
            "Bearer",
            expiresAt,
            new LoginUsuarioResponse(usuario.Id, usuario.Nome, usuario.Email, usuario.Role)));
    }
}
