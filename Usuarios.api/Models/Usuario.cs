namespace Usuarios.Api.Models;

public class Usuario
{
    public string NombreUsuario { get; set; } = string.Empty;
    public int ItemsCompletados { get; set; }
    public int ItemsPendientesTotal { get; set; }
    public int ItemsPendientesAltaRelevancia { get; set; }
}