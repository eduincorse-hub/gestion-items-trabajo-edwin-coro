namespace Usuarios.Api.DTOs;

public record CrearUsuarioDto(string NombreUsuario);
public record UsuarioResumenDto(
    string NombreUsuario,
    int ItemsCompletados,
    int ItemsPendientesTotal,
    int ItemsPendientesAltaRelevancia);

public record NotificacionItemDto(bool EsAltaRelevancia);