using DamMonitor.Host.Contracts;
using DamMonitor.Host.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DamMonitor.Host.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/iot")]
public sealed class IotController(IotIngestionService iotIngestionService) : ControllerBase
{
    [HttpPost("leituras")]
    public async Task<IActionResult> ReceiveReading(IotReadingRequest request)
    {
        var result = await iotIngestionService.ReceiveAsync(request);
        return result.IsSuccess ? Accepted(value: result.Value) : BadRequest(new { erro = result.Error });
    }
}
