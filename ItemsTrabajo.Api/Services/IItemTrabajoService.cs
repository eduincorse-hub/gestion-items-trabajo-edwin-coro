using ItemsTrabajo.Api.Clients;
using ItemsTrabajo.Api.DTOs;
using ItemsTrabajo.Api.Models;
using ItemsTrabajo.Api.Repositories;

namespace ItemsTrabajo.Api.Services;

public interface IItemTrabajoService
{
    IReadOnlyCollection<ItemTrabajoDto> ObtenerTodos();
    ItemTrabajoDto Crear(CrearItemDto dto);
    Task<ResultadoDistribucionDto> DistribuirPendientesAsync();
    Task<ItemTrabajoDto?> CompletarAsync(Guid id);
}

public class ItemTrabajoService : IItemTrabajoService
{
    private readonly IItemTrabajoRepository _repositorio;
    private readonly IMotorDistribucion _motorDistribucion;
    private readonly IUsuariosApiClient _usuariosApiClient;

    public ItemTrabajoService(
        IItemTrabajoRepository repositorio,
        IMotorDistribucion motorDistribucion,
        IUsuariosApiClient usuariosApiClient)
    {
        _repositorio = repositorio;
        _motorDistribucion = motorDistribucion;
        _usuariosApiClient = usuariosApiClient;
    }

    public IReadOnlyCollection<ItemTrabajoDto> ObtenerTodos() =>
        _repositorio.ObtenerTodos().Select(Mapear).ToList();

    public ItemTrabajoDto Crear(CrearItemDto dto)
    {
        var item = new ItemTrabajo
        {
            Titulo = dto.Titulo,
            Descripcion = dto.Descripcion,
            FechaEntrega = dto.FechaEntrega,
            Relevancia = dto.Relevancia
        };

        _repositorio.Crear(item);
        return Mapear(item);
    }

    public async Task<ResultadoDistribucionDto> DistribuirPendientesAsync()
    {
        var pendientes = _repositorio.ObtenerPendientesSinAsignar();
        var usuarios = await _usuariosApiClient.ObtenerUsuariosAsync();

        var resultado = _motorDistribucion.Distribuir(pendientes, usuarios);

        var asignacionesDto = new List<AsignacionRealizadaDto>();

        foreach (var asignacion in resultado.Asignaciones)
        {
            var item = _repositorio.ObtenerPorId(asignacion.ItemId)!;
            item.UsuarioAsignado = asignacion.UsuarioAsignado;
            item.FechaAsignacion = DateTime.UtcNow;
            _repositorio.Actualizar(item);

            await _usuariosApiClient.NotificarAsignacionAsync(
                asignacion.UsuarioAsignado, asignacion.EsAltaRelevancia);

            asignacionesDto.Add(new AsignacionRealizadaDto(item.Id, item.Titulo, asignacion.UsuarioAsignado));
        }

        var sinAsignarTitulos = resultado.SinAsignar
            .Select(id => _repositorio.ObtenerPorId(id)?.Titulo ?? id.ToString())
            .ToList();

        return new ResultadoDistribucionDto(asignacionesDto, sinAsignarTitulos);
    }

    public async Task<ItemTrabajoDto?> CompletarAsync(Guid id)
    {
        var item = _repositorio.ObtenerPorId(id);
        if (item is null) return null;

        item.Estado = EstadoItem.Completado;
        _repositorio.Actualizar(item);

        if (item.UsuarioAsignado is not null)
        {
            await _usuariosApiClient.NotificarCompletadoAsync(
                item.UsuarioAsignado, item.Relevancia == Relevancia.Alta);
        }

        return Mapear(item);
    }

    private static ItemTrabajoDto Mapear(ItemTrabajo i) =>
        new(i.Id, i.Titulo, i.Descripcion, i.FechaEntrega, i.Relevancia, i.Estado, i.UsuarioAsignado);
}