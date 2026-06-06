using DamMonitor.Host.Contracts;
using DamMonitor.Host.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DamMonitor.Host.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/autenticacao")]
public sealed class AuthController(AuthService authService) : ControllerBase
{
    [HttpPost("entrar")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await authService.LoginAsync(request);
        if (!result.IsSuccess)
        {
            return Unauthorized();
        }

        return Ok(result.Value);
    }
}
