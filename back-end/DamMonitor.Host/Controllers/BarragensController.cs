using DamMonitor.Host.Contracts;
using DamMonitor.Host.Services;
using Microsoft.AspNetCore.Mvc;

namespace DamMonitor.Host.Controllers;

[ApiController]
[Route("api/barragens")]
public sealed class BarragensController(BarragensService barragensService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await barragensService.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var barragem = await barragensService.GetByIdAsync(id);
        return barragem is null ? NotFound() : Ok(barragem);
    }

    [HttpPost]
    public async Task<IActionResult> Create(BarragemRequest request)
    {
        var result = await barragensService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, BarragemRequest request)
    {
        var result = await barragensService.UpdateAsync(id, request);
        return ToActionResult(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await barragensService.DeleteAsync(id);
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
