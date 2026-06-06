using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DamMonitor.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace DamMonitor.Host.Services;

public sealed class JwtTokenService(IConfiguration configuration)
{
    public string Generate(Usuario usuario, DateTime expiresAt)
    {
        var secret = configuration.GetValue<string>("Jwt:Secret")
            ?? throw new InvalidOperationException("Jwt:Secret não foi configurado.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim(ClaimTypes.Role, usuario.Role)
        };

        var token = new JwtSecurityToken(
            issuer: configuration.GetValue<string>("Jwt:Issuer") ?? "DamMonitor",
            audience: configuration.GetValue<string>("Jwt:Audience") ?? "DamMonitor.Mobile",
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
