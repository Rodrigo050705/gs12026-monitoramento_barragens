using DamMonitor.Host.Contracts;
using DamMonitor.Host.Services;
using Microsoft.AspNetCore.Mvc;

namespace DamMonitor.Host.Controllers;

[ApiController]
[Route("api/usuarios")]
public sealed class UsuariosController(UsuariosService usuariosService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await usuariosService.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var usuario = await usuariosService.GetByIdAsync(id);
        return usuario is null ? NotFound() : Ok(usuario);
    }

    [HttpPost]
    public async Task<IActionResult> Create(UsuarioRequest request)
    {
        var result = await usuariosService.CreateAsync(request);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : ToActionResult(result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UsuarioRequest request)
    {
        var result = await usuariosService.UpdateAsync(id, request);
        return ToActionResult(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await usuariosService.DeleteAsync(id);
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
