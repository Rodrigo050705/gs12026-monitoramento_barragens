using DamMonitor.Host.Contracts;
using DamMonitor.Host.Services;
using Microsoft.AspNetCore.Mvc;

namespace DamMonitor.Host.Controllers;

[ApiController]
[Route("api/sensores")]
public sealed class SensoresController(SensoresService sensoresService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await sensoresService.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var sensor = await sensoresService.GetByIdAsync(id);
        return sensor is null ? NotFound() : Ok(sensor);
    }

    [HttpGet("{codigoIdentificador}/leituras")]
    public async Task<IActionResult> GetReadings(string codigoIdentificador, [FromQuery(Name = "quantidade")] int quantidade = 100)
    {
        return Ok(await sensoresService.GetReadingsAsync(codigoIdentificador, quantidade));
    }

    [HttpPost]
    public async Task<IActionResult> Create(SensorRequest request)
    {
        var result = await sensoresService.CreateAsync(request);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : ToActionResult(result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, SensorRequest request)
    {
        var result = await sensoresService.UpdateAsync(id, request);
        return ToActionResult(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await sensoresService.DeleteAsync(id);
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
