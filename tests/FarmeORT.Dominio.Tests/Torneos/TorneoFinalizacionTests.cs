using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.Excepciones;

namespace FarmeORT.Dominio.Tests.Torneos;

[TestClass]
public sealed class TorneoFinalizacionTests
{
    private static readonly DateOnly FechaActual = new(2026, 10, 2);

    private Torneo _torneo = null!;
    private Usuario _campeon = null!;

    [TestInitialize]
    public void Inicializar()
    {
        var organizador = new Usuario("Laura", "Gomez", "laura@farmeort.com", "laurag", new DateOnly(1990, 5, 20), Rol.Organizador, "hash", FechaActual)
        {
            Id = 2,
        };
        _torneo = new Torneo("COPAAURA", "Copa Aura", "Torneo eliminatorio de batallas de aura.", FechaActual, FechaActual.AddDays(10), 4, organizador, FechaActual);
        _campeon = new Usuario("Ana", "Pérez", "ana@farmeort.com", "aurafarmer", new DateOnly(2000, 1, 15), Rol.Jugador, "hash", FechaActual)
        {
            Id = 7,
        };
    }

    [TestMethod]
    public void Finalizar_TorneoAbierto_LanzaTorneoNoEnCurso()
    {
        Assert.ThrowsExactly<TorneoNoEnCursoException>(() => _torneo.Finalizar(_campeon));
        Assert.AreEqual(EstadoTorneo.Abierto, _torneo.Estado);
        Assert.IsNull(_torneo.CampeonId);
    }

    [TestMethod]
    public void Finalizar_TorneoEnCurso_QuedaFinalizadoConSuCampeon()
    {
        _torneo.Iniciar(4);

        _torneo.Finalizar(_campeon);

        Assert.AreEqual(EstadoTorneo.Finalizado, _torneo.Estado);
        Assert.AreSame(_campeon, _torneo.Campeon);
        Assert.AreEqual(7, _torneo.CampeonId);
    }

    [TestMethod]
    public void Finalizar_TorneoYaFinalizado_LanzaTorneoNoEnCurso()
    {
        _torneo.Iniciar(4);
        _torneo.Finalizar(_campeon);

        Assert.ThrowsExactly<TorneoNoEnCursoException>(() => _torneo.Finalizar(_campeon));
    }

    [TestMethod]
    public void Cancelar_TorneoFinalizado_LanzaTorneoNoCancelable()
    {
        _torneo.Iniciar(4);
        _torneo.Finalizar(_campeon);

        Assert.ThrowsExactly<TorneoNoCancelableException>(_torneo.Cancelar);
        Assert.AreEqual(EstadoTorneo.Finalizado, _torneo.Estado);
    }

    [TestMethod]
    public void Modificar_TorneoFinalizado_LanzaTorneoNoModificable()
    {
        _torneo.Iniciar(4);
        _torneo.Finalizar(_campeon);

        Assert.ThrowsExactly<TorneoNoModificableException>(
            () => _torneo.Modificar("COPAFUEGO", "Copa Fuego", "Torneo eliminatorio de batallas de aura.", FechaActual, FechaActual.AddDays(10), 8, _torneo.Organizador, FechaActual, 0));
        Assert.AreEqual("COPAAURA", _torneo.Codigo);
        Assert.AreEqual(EstadoTorneo.Finalizado, _torneo.Estado);
    }
}
