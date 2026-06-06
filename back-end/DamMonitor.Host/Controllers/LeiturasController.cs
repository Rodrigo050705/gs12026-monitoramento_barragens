using DamMonitor.Host.Contracts;
using DamMonitor.Host.Services;
using Microsoft.AspNetCore.Mvc;

namespace DamMonitor.Host.Controllers;

[ApiController]
[Route("api/leituras")]
public sealed class LeiturasController(LeiturasService leiturasService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery(Name = "quantidade")] int quantidade = 100)
    {
        return Ok(await leiturasService.GetAllAsync(quantidade));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var leitura = await leiturasService.GetByIdAsync(id);
        return leitura is null ? NotFound() : Ok(leitura);
    }

    [HttpPost]
    public async Task<IActionResult> Create(LeituraRequest request)
    {
        var result = await leiturasService.CreateAsync(request);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : ToActionResult(result);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, LeituraRequest request)
    {
        var result = await leiturasService.UpdateAsync(id, request);
        return ToActionResult(result);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await leiturasService.DeleteAsync(id);
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
