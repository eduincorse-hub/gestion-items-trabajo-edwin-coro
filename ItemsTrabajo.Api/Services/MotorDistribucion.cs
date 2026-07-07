using ItemsTrabajo.Api.DTOs;
using ItemsTrabajo.Api.Models;

namespace ItemsTrabajo.Api.Services;

public record AsignacionInterna(Guid ItemId, string UsuarioAsignado, bool EsAltaRelevancia);

public record ResultadoDistribucionInterno(List<AsignacionInterna> Asignaciones, List<Guid> SinAsignar);

public interface IMotorDistribucion
{
    ResultadoDistribucionInterno Distribuir(
        IReadOnlyCollection<ItemTrabajo> items,
        IReadOnlyCollection<UsuarioResumenDto> usuarios);
}

internal class UsuarioSimulado
{
    public string NombreUsuario { get; init; } = string.Empty;
    public int ItemsPendientesTotal { get; set; }
    public int ItemsPendientesAltaRelevancia { get; set; }

    public static UsuarioSimulado DesdeDto(UsuarioResumenDto dto) => new()
    {
        NombreUsuario = dto.NombreUsuario,
        ItemsPendientesTotal = dto.ItemsPendientesTotal,
        ItemsPendientesAltaRelevancia = dto.ItemsPendientesAltaRelevancia
    };
}

public class MotorDistribucion : IMotorDistribucion
{
    private const int DiasParaConsiderarUrgente = 3;
    private const int LimiteAltaRelevanciaSaturacion = 3;

    public ResultadoDistribucionInterno Distribuir(
        IReadOnlyCollection<ItemTrabajo> items,
        IReadOnlyCollection<UsuarioResumenDto> usuarios)
    {
        var asignaciones = new List<AsignacionInterna>();
        var sinAsignar = new List<Guid>();

        var usuariosSimulados = usuarios.Select(UsuarioSimulado.DesdeDto).ToList();

        var cola = items
            .OrderByDescending(i => EsUrgente(i.FechaEntrega))
            .ThenByDescending(i => i.Relevancia == Relevancia.Alta)
            .ThenBy(i => i.FechaEntrega)
            .ToList();

        foreach (var item in cola)
        {
            var candidatos = usuariosSimulados.Where(u => !EstaSaturado(u)).ToList();

            if (candidatos.Count == 0)
            {
                sinAsignar.Add(item.Id);
                continue;
            }

            var elegido = candidatos.OrderBy(u => u.ItemsPendientesTotal).First();

            elegido.ItemsPendientesTotal++;
            if (item.Relevancia == Relevancia.Alta)
                elegido.ItemsPendientesAltaRelevancia++;

            asignaciones.Add(new AsignacionInterna(
                item.Id, elegido.NombreUsuario, item.Relevancia == Relevancia.Alta));

            usuariosSimulados = usuariosSimulados
                .OrderBy(u => u.ItemsPendientesTotal)
                .ToList();
        }

        return new ResultadoDistribucionInterno(asignaciones, sinAsignar);
    }

    private static bool EsUrgente(DateTime fechaEntrega) =>
        (fechaEntrega.Date - DateTime.UtcNow.Date).TotalDays < DiasParaConsiderarUrgente;

    private static bool EstaSaturado(UsuarioSimulado usuario) =>
        usuario.ItemsPendientesAltaRelevancia > LimiteAltaRelevanciaSaturacion;
}