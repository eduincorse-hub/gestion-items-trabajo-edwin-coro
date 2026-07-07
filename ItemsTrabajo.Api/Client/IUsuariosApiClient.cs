using System.Net.Http.Json;
using ItemsTrabajo.Api.DTOs;

namespace ItemsTrabajo.Api.Clients;

public interface IUsuariosApiClient
{
    Task<List<UsuarioResumenDto>> ObtenerUsuariosAsync();
    Task NotificarAsignacionAsync(string nombreUsuario, bool esAltaRelevancia);
    Task NotificarCompletadoAsync(string nombreUsuario, bool eraAltaRelevancia);
}

public class UsuariosApiClient : IUsuariosApiClient
{
    private readonly HttpClient _http;

    public UsuariosApiClient(HttpClient http) => _http = http;

    public async Task<List<UsuarioResumenDto>> ObtenerUsuariosAsync()
    {
        var usuarios = await _http.GetFromJsonAsync<List<UsuarioResumenDto>>("api/usuarios");
        return usuarios ?? new List<UsuarioResumenDto>();
    }

    public async Task NotificarAsignacionAsync(string nombreUsuario, bool esAltaRelevancia)
    {
        var respuesta = await _http.PostAsJsonAsync(
            $"api/usuarios/{nombreUsuario}/asignaciones",
            new { EsAltaRelevancia = esAltaRelevancia });
        respuesta.EnsureSuccessStatusCode();
    }

    public async Task NotificarCompletadoAsync(string nombreUsuario, bool eraAltaRelevancia)
    {
        var respuesta = await _http.PostAsJsonAsync(
            $"api/usuarios/{nombreUsuario}/completados",
            new { EsAltaRelevancia = eraAltaRelevancia });
        respuesta.EnsureSuccessStatusCode();
    }
}