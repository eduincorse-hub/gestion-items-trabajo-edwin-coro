using ItemsTrabajo.Api.DTOs;
using ItemsTrabajo.Api.Models;
using ItemsTrabajo.Api.Services;
using Xunit;

namespace ItemsTrabajo.Api.Tests;

public class MotorDistribucionTests
{
    private readonly MotorDistribucion _motor = new();

    private static ItemTrabajo CrearItem(string titulo, DateTime fechaEntrega, Relevancia relevancia) =>
        new()
        {
            Titulo = titulo,
            FechaEntrega = fechaEntrega,
            Relevancia = relevancia
        };

    private static UsuarioResumenDto CrearUsuario(
        string nombre, int pendientesTotal = 0, int pendientesAlta = 0) =>
        new(nombre, ItemsCompletados: 0, pendientesTotal, pendientesAlta);

    [Fact]
    public void Item_urgente_se_asigna_al_usuario_con_menos_pendientes_sin_importar_relevancia()
    {
        // Arrange: item urgente (vence manana) de relevancia Baja
        var item = CrearItem("Urgente", DateTime.UtcNow.AddDays(1), Relevancia.Baja);

        var usuarios = new List<UsuarioResumenDto>
        {
            CrearUsuario("ana", pendientesTotal: 3),
            CrearUsuario("beto", pendientesTotal: 0) // deberia ganar por tener menos pendientes
        };

        // Act
        var resultado = _motor.Distribuir(new[] { item }, usuarios);

        // Assert
        Assert.Single(resultado.Asignaciones);
        Assert.Equal("beto", resultado.Asignaciones[0].UsuarioAsignado);
    }

    [Fact]
    public void Item_de_relevancia_alta_no_urgente_se_asigna_a_quien_tiene_menos_pendientes()
    {
        // Arrange: item NO urgente (vence en 30 dias) de relevancia Alta
        var item = CrearItem("Alta relevancia", DateTime.UtcNow.AddDays(30), Relevancia.Alta);

        var usuarios = new List<UsuarioResumenDto>
        {
            CrearUsuario("ana", pendientesTotal: 1),
            CrearUsuario("beto", pendientesTotal: 4)
        };

        // Act
        var resultado = _motor.Distribuir(new[] { item }, usuarios);

        // Assert
        Assert.Equal("ana", resultado.Asignaciones[0].UsuarioAsignado);
    }

    [Fact]
    public void Usuario_saturado_con_mas_de_3_alta_relevancia_queda_excluido()
    {
        // Arrange: "saturado" ya tiene 4 pendientes de alta relevancia (> 3)
        var item = CrearItem("Nueva tarea alta", DateTime.UtcNow.AddDays(30), Relevancia.Alta);

        var usuarios = new List<UsuarioResumenDto>
        {
            CrearUsuario("saturado", pendientesTotal: 4, pendientesAlta: 4),
            CrearUsuario("disponible", pendientesTotal: 10, pendientesAlta: 0)
        };

        // Act
        var resultado = _motor.Distribuir(new[] { item }, usuarios);

        // Assert: aunque "disponible" tiene mas pendientes en total,
        // "saturado" esta excluido, asi que debe ganar "disponible".
        Assert.Equal("disponible", resultado.Asignaciones[0].UsuarioAsignado);
    }

    [Fact]
    public void Item_queda_sin_asignar_si_todos_los_usuarios_estan_saturados()
    {
        // Arrange: los dos unicos usuarios estan saturados
        var item = CrearItem("Sin destino posible", DateTime.UtcNow.AddDays(30), Relevancia.Alta);

        var usuarios = new List<UsuarioResumenDto>
        {
            CrearUsuario("ana", pendientesAlta: 4),
            CrearUsuario("beto", pendientesAlta: 5)
        };

        // Act
        var resultado = _motor.Distribuir(new[] { item }, usuarios);

        // Assert
        Assert.Empty(resultado.Asignaciones);
        Assert.Single(resultado.SinAsignar);
    }

    [Fact]
    public void Relevancia_alta_se_procesa_antes_que_relevancia_baja_cuando_ninguno_es_urgente()
    {
        // Arrange: dos items no urgentes, uno Baja y otro Alta, un solo usuario disponible
        var itemBaja = CrearItem("Tarea baja", DateTime.UtcNow.AddDays(30), Relevancia.Baja);
        var itemAlta = CrearItem("Tarea alta", DateTime.UtcNow.AddDays(30), Relevancia.Alta);

        var usuarios = new List<UsuarioResumenDto> { CrearUsuario("ana") };

        // Act: se pasan en orden Baja, Alta a proposito
        var resultado = _motor.Distribuir(new[] { itemBaja, itemAlta }, usuarios);

        // Assert: el primero en procesarse (y por lo tanto el primero en la
        // lista de asignaciones) debe ser el item de relevancia Alta.
        Assert.Equal(itemAlta.Id, resultado.Asignaciones[0].ItemId);
    }

    [Fact]
    public void Lista_de_usuarios_se_reordena_despues_de_cada_asignacion()
    {
        // Arrange: dos items no urgentes de relevancia Alta, dos usuarios que
        // arrancan empatados en pendientes.
        var item1 = CrearItem("Item 1", DateTime.UtcNow.AddDays(30), Relevancia.Alta);
        var item2 = CrearItem("Item 2", DateTime.UtcNow.AddDays(30), Relevancia.Alta);

        var usuarios = new List<UsuarioResumenDto>
        {
            CrearUsuario("ana", pendientesTotal: 0),
            CrearUsuario("beto", pendientesTotal: 0)
        };

        // Act
        var resultado = _motor.Distribuir(new[] { item1, item2 }, usuarios);

        // Assert: como ambos arrancan en 0, el primer item se lo lleva alguno,
        // y el segundo debe ir al OTRO usuario (porque tras la 1ra asignacion
        // ese quedo con menos pendientes).
        Assert.Equal(2, resultado.Asignaciones.Count);
        Assert.NotEqual(
            resultado.Asignaciones[0].UsuarioAsignado,
            resultado.Asignaciones[1].UsuarioAsignado);
    }
}