using DamMonitor.Host.Contracts;
using DamMonitor.Host.Services;
using Microsoft.AspNetCore.Mvc;

namespace DamMonitor.Host.Controllers;

[ApiController]
[Route("api/alertas")]
public sealed class AlertasController(AlertasService alertasService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await alertasService.GetAllAsync());
    }

    [HttpGet("ativos")]
    public async Task<IActionResult> GetActive()
    {
        return Ok(await alertasService.GetActiveAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var alerta = await alertasService.GetByIdAsync(id);
        return alerta is null ? NotFound() : Ok(alerta);
    }

    [HttpPost]
    public async Task<IActionResult> Create(AlertaRequest request)
    {
        var result = await alertasService.CreateAsync(request);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : ToActionResult(result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, AlertaRequest request)
    {
        var result = await alertasService.UpdateAsync(id, request);
        return ToActionResult(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await alertasService.DeleteAsync(id);
        return ToActionResult(result, noContentOnSuccess: true);
    }

    private IActionResult ToActionResult<T>(ServiceResult<T> result, bool noContentOnSuccess = false)
    {
        return result.Status switch
        {
            ServiceResultStatus.Success => noContentOnSuccess ? NoContent() : Ok(result.Value),
            ServiceResultStatus.NotFound => NotFound(new { erro = result.Error }),
            ServiceResultStatus.Conflict => Conflict(new { erro = result.Error }),
            ServiceResultStatus.BadRequest => BadRequest(new { erro = result.Error }),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }
}
