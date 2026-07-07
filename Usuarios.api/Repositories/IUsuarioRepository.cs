using System.Collections.Concurrent;
using Usuarios.Api.Models;

namespace Usuarios.Api.Repositories;

public interface IUsuarioRepository
{
    IReadOnlyCollection<Usuario> ObtenerTodos();
    Usuario? ObtenerPorNombre(string nombreUsuario);
    Usuario Crear(string nombreUsuario);
    void Actualizar(Usuario usuario);
}

public class UsuarioRepository : IUsuarioRepository
{
    private readonly ConcurrentDictionary<string, Usuario> _usuarios =
        new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<Usuario> ObtenerTodos() => _usuarios.Values.ToList();

    public Usuario? ObtenerPorNombre(string nombreUsuario) =>
        _usuarios.TryGetValue(nombreUsuario, out var usuario) ? usuario : null;

    public Usuario Crear(string nombreUsuario)
    {
        var usuario = new Usuario { NombreUsuario = nombreUsuario };
        _usuarios[nombreUsuario] = usuario;
        return usuario;
    }

    public void Actualizar(Usuario usuario) => _usuarios[usuario.NombreUsuario] = usuario;
}