using Microsoft.AspNetCore.Mvc;
using Usuarios.Api.DTOs;
using Usuarios.Api.Services;

namespace Usuarios.Api.Controllers;

/// <summary>
/// Microservicio de Gestion de Usuarios. Expone el seguimiento de items completados/pendientes
/// de cada usuario.Api para ejecutar el algoritmo de distribucion.
/// </summary>

[ApiController]
[Route("api/usuarios")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _servicio;

    public UsuariosController(IUsuarioService servicio) => _servicio = servicio;

    /// <summary>
    /// Devuelve todos los usuarios registrados, ordenados por cantidad de
    /// items pendientes (de menor a mayor).
    /// </summary>

    [HttpGet]
    public ActionResult<IReadOnlyCollection<UsuarioResumenDto>> ObtenerTodos() =>
        Ok(_servicio.ObtenerTodos());

    [HttpGet("{nombreUsuario}")]
    public ActionResult<UsuarioResumenDto> ObtenerPorNombre(string nombreUsuario)
    {
        var usuario = _servicio.ObtenerPorNombre(nombreUsuario);
        return usuario is null ? NotFound() : Ok(usuario);
    }

    /// <summary>
    /// Registra la referencia de un usuario que ya existe en el sistema
    /// externo, para empezar a llevarle seguimiento de items.
    /// </summary>

    [HttpPost]
    public ActionResult<UsuarioResumenDto> Crear(CrearUsuarioDto dto)
    {
        var usuario = _servicio.CrearSiNoExiste(dto.NombreUsuario);
        return CreatedAtAction(nameof(ObtenerPorNombre), new { nombreUsuario = usuario.NombreUsuario }, usuario);
    }

    /// <summary>
    /// Llamado por ItemsTrabajo.Api cuando el algoritmo de distribucion le
    /// asigna un nuevo item a este usuario.
    /// </summary>

    [HttpPost("{nombreUsuario}/asignaciones")]
    public ActionResult<UsuarioResumenDto> RegistrarAsignacion(string nombreUsuario, NotificacionItemDto dto)
    {
        var usuario = _servicio.RegistrarAsignacion(nombreUsuario, dto.EsAltaRelevancia);
        return usuario is null ? NotFound() : Ok(usuario);
    }

    /// <summary>
    /// Llamado por ItemsTrabajo.Api cuando este usuario completa un item.
    /// Mueve el contador de pendientes a completados.
    /// </summary>

    [HttpPost("{nombreUsuario}/completados")]
    public ActionResult<UsuarioResumenDto> RegistrarCompletado(string nombreUsuario, NotificacionItemDto dto)
    {
        var usuario = _servicio.RegistrarCompletado(nombreUsuario, dto.EsAltaRelevancia);
        return usuario is null ? NotFound() : Ok(usuario);
    }
}