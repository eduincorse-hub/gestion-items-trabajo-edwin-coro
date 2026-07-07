using ItemsTrabajo.Api.Models;

namespace ItemsTrabajo.Api.DTOs;

public record CrearItemDto(string Titulo, string? Descripcion, DateTime FechaEntrega, Relevancia Relevancia);

public record ItemTrabajoDto(
    Guid Id,
    string Titulo,
    string? Descripcion,
    DateTime FechaEntrega,
    Relevancia Relevancia,
    EstadoItem Estado,
    string? UsuarioAsignado);

public record UsuarioResumenDto(
    string NombreUsuario,
    int ItemsCompletados,
    int ItemsPendientesTotal,
    int ItemsPendientesAltaRelevancia);

public record AsignacionRealizadaDto(Guid ItemId, string Titulo, string UsuarioAsignado);

public record ResultadoDistribucionDto(
    List<AsignacionRealizadaDto> Asignaciones,
    List<string> ItemsSinAsignar);