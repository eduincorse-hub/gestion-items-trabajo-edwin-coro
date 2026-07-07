using System.Collections.Concurrent;
using ItemsTrabajo.Api.Models;

namespace ItemsTrabajo.Api.Repositories;

public interface IItemTrabajoRepository
{
    IReadOnlyCollection<ItemTrabajo> ObtenerTodos();
    IReadOnlyCollection<ItemTrabajo> ObtenerPendientesSinAsignar();
    ItemTrabajo? ObtenerPorId(Guid id);
    ItemTrabajo Crear(ItemTrabajo item);
    void Actualizar(ItemTrabajo item);
}

public class ItemTrabajoRepository : IItemTrabajoRepository
{
    private readonly ConcurrentDictionary<Guid, ItemTrabajo> _items = new();

    public IReadOnlyCollection<ItemTrabajo> ObtenerTodos() => _items.Values.ToList();

    public IReadOnlyCollection<ItemTrabajo> ObtenerPendientesSinAsignar() =>
        _items.Values
            .Where(i => i.Estado == EstadoItem.Pendiente && i.UsuarioAsignado is null)
            .ToList();

    public ItemTrabajo? ObtenerPorId(Guid id) => _items.TryGetValue(id, out var item) ? item : null;

    public ItemTrabajo Crear(ItemTrabajo item)
    {
        _items[item.Id] = item;
        return item;
    }

    public void Actualizar(ItemTrabajo item) => _items[item.Id] = item;
}