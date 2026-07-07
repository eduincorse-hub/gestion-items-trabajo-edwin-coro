using Microsoft.AspNetCore.Mvc;
using ItemsTrabajo.Api.DTOs;
using ItemsTrabajo.Api.Services;

namespace ItemsTrabajo.Api.Controllers;

/// <summary>
/// Microservicio de Gestion de Items de Trabajo administra el ciclo de vida
/// de los items.
/// </summary>

[ApiController]
[Route("api/items")]
public class ItemsTrabajoController : ControllerBase
{
    private readonly IItemTrabajoService _servicio;

    public ItemsTrabajoController(IItemTrabajoService servicio) => _servicio = servicio;

    [HttpGet]
    public ActionResult<IReadOnlyCollection<ItemTrabajoDto>> ObtenerTodos() =>
        Ok(_servicio.ObtenerTodos());

    /// <summary>
    /// Crea un nuevo item de trabajo en estado Pendiente y sin asignar.
    /// </summary>

    [HttpPost]
    public ActionResult<ItemTrabajoDto> Crear(CrearItemDto dto)
    {
        var item = _servicio.Crear(dto);
        return CreatedAtAction(nameof(ObtenerTodos), item);
    }

    [HttpPost("distribuir")]
    public async Task<ActionResult<ResultadoDistribucionDto>> Distribuir()
    {
        var resultado = await _servicio.DistribuirPendientesAsync();
        return Ok(resultado);
    }

    /// <summary>
    /// Marca un item como completado y notifica al microservicio de Usuarios
    /// para que mueva el contador de pendientes a completados.
    /// </summary>

    [HttpPut("{id}/completar")]
    public async Task<ActionResult<ItemTrabajoDto>> Completar(Guid id)
    {
        var item = await _servicio.CompletarAsync(id);
        return item is null ? NotFound() : Ok(item);
    }
}