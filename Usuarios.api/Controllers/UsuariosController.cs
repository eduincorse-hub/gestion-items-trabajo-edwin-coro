using Microsoft.AspNetCore.Mvc;
using Usuarios.Api.DTOs;
using Usuarios.Api.Services;

namespace Usuarios.Api.Controllers;

[ApiController]
[Route("api/usuarios")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _servicio;

    public UsuariosController(IUsuarioService servicio) => _servicio = servicio;

    [HttpGet]
    public ActionResult<IReadOnlyCollection<UsuarioResumenDto>> ObtenerTodos() =>
        Ok(_servicio.ObtenerTodos());

    [HttpGet("{nombreUsuario}")]
    public ActionResult<UsuarioResumenDto> ObtenerPorNombre(string nombreUsuario)
    {
        var usuario = _servicio.ObtenerPorNombre(nombreUsuario);
        return usuario is null ? NotFound() : Ok(usuario);
    }

    [HttpPost]
    public ActionResult<UsuarioResumenDto> Crear(CrearUsuarioDto dto)
    {
        var usuario = _servicio.CrearSiNoExiste(dto.NombreUsuario);
        return CreatedAtAction(nameof(ObtenerPorNombre), new { nombreUsuario = usuario.NombreUsuario }, usuario);
    }

    [HttpPost("{nombreUsuario}/asignaciones")]
    public ActionResult<UsuarioResumenDto> RegistrarAsignacion(string nombreUsuario, NotificacionItemDto dto)
    {
        var usuario = _servicio.RegistrarAsignacion(nombreUsuario, dto.EsAltaRelevancia);
        return usuario is null ? NotFound() : Ok(usuario);
    }

    [HttpPost("{nombreUsuario}/completados")]
    public ActionResult<UsuarioResumenDto> RegistrarCompletado(string nombreUsuario, NotificacionItemDto dto)
    {
        var usuario = _servicio.RegistrarCompletado(nombreUsuario, dto.EsAltaRelevancia);
        return usuario is null ? NotFound() : Ok(usuario);
    }
}