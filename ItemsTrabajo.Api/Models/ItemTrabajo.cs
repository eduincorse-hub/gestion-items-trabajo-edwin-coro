namespace ItemsTrabajo.Api.Models;

public enum Relevancia
{
    Baja = 0,
    Alta = 1
}

public enum EstadoItem
{
    Pendiente = 0,
    Completado = 1
}

public class ItemTrabajo
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public DateTime FechaEntrega { get; set; }
    public Relevancia Relevancia { get; set; }
    public EstadoItem Estado { get; set; } = EstadoItem.Pendiente;
    public string? UsuarioAsignado { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaAsignacion { get; set; }
}