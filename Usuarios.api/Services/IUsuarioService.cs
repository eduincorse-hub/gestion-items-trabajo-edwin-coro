using Usuarios.Api.DTOs;
using Usuarios.Api.Models;
using Usuarios.Api.Repositories;

namespace Usuarios.Api.Services;

public interface IUsuarioService
{
    IReadOnlyCollection<UsuarioResumenDto> ObtenerTodos();
    UsuarioResumenDto? ObtenerPorNombre(string nombreUsuario);
    UsuarioResumenDto CrearSiNoExiste(string nombreUsuario);
    UsuarioResumenDto? RegistrarAsignacion(string nombreUsuario, bool esAltaRelevancia);
    UsuarioResumenDto? RegistrarCompletado(string nombreUsuario, bool eraAltaRelevancia);
}

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _repositorio;

    public UsuarioService(IUsuarioRepository repositorio) => _repositorio = repositorio;

    public IReadOnlyCollection<UsuarioResumenDto> ObtenerTodos() =>
        _repositorio.ObtenerTodos()
            .OrderBy(u => u.ItemsPendientesTotal)
            .Select(Mapear)
            .ToList();

    public UsuarioResumenDto? ObtenerPorNombre(string nombreUsuario)
    {
        var usuario = _repositorio.ObtenerPorNombre(nombreUsuario);
        return usuario is null ? null : Mapear(usuario);
    }

    public UsuarioResumenDto CrearSiNoExiste(string nombreUsuario)
    {
        var existente = _repositorio.ObtenerPorNombre(nombreUsuario);
        return existente is not null ? Mapear(existente) : Mapear(_repositorio.Crear(nombreUsuario));
    }

    public UsuarioResumenDto? RegistrarAsignacion(string nombreUsuario, bool esAltaRelevancia)
    {
        var usuario = _repositorio.ObtenerPorNombre(nombreUsuario);
        if (usuario is null) return null;

        usuario.ItemsPendientesTotal++;
        if (esAltaRelevancia) usuario.ItemsPendientesAltaRelevancia++;

        _repositorio.Actualizar(usuario);
        return Mapear(usuario);
    }

    public UsuarioResumenDto? RegistrarCompletado(string nombreUsuario, bool eraAltaRelevancia)
    {
        var usuario = _repositorio.ObtenerPorNombre(nombreUsuario);
        if (usuario is null) return null;

        if (usuario.ItemsPendientesTotal > 0) usuario.ItemsPendientesTotal--;
        if (eraAltaRelevancia && usuario.ItemsPendientesAltaRelevancia > 0) usuario.ItemsPendientesAltaRelevancia--;
        usuario.ItemsCompletados++;

        _repositorio.Actualizar(usuario);
        return Mapear(usuario);
    }

    private static UsuarioResumenDto Mapear(Usuario u) =>
        new(u.NombreUsuario, u.ItemsCompletados, u.ItemsPendientesTotal, u.ItemsPendientesAltaRelevancia);
}