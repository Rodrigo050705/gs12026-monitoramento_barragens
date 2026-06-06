using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DamMonitor.Host.Controllers;

[ApiController]
[AllowAnonymous]
[Route("saude")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { estado = "Operacional" });
}
