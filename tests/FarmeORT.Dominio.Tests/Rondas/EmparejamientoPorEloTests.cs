using FarmeORT.Dominio.Rondas;
using FarmeORT.Dominio.Usuarios;

namespace FarmeORT.Dominio.Tests.Rondas;

[TestClass]
public sealed class EmparejamientoPorEloTests
{
    private static readonly DateOnly FechaActual = new(2026, 10, 2);

    private EmparejamientoPorElo _estrategia = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _estrategia = new EmparejamientoPorElo();
    }

    [TestMethod]
    public void Emparejar_DosJugadores_LosEnfrentaEntreSi()
    {
        var primero = CrearJugador(1);
        var segundo = CrearJugador(2);

        var pares = _estrategia.Emparejar([primero, segundo]);

        Assert.HasCount(1, pares);
        Assert.AreSame(primero, pares[0].Jugador1);
        Assert.AreSame(segundo, pares[0].Jugador2);
    }

    [TestMethod]
    public void Emparejar_EloIgual_EnfrentaAlPrimeroConElUltimoOrdenadosPorId()
    {
        var jugador1 = CrearJugador(1);
        var jugador2 = CrearJugador(2);
        var jugador3 = CrearJugador(3);
        var jugador4 = CrearJugador(4);

        var pares = _estrategia.Emparejar([jugador3, jugador1, jugador4, jugador2]);

        Assert.HasCount(2, pares);
        Assert.AreSame(jugador1, pares[0].Jugador1);
        Assert.AreSame(jugador4, pares[0].Jugador2);
        Assert.AreSame(jugador2, pares[1].Jugador1);
        Assert.AreSame(jugador3, pares[1].Jugador2);
    }

    [TestMethod]
    public void Emparejar_EloDistinto_EnfrentaAlDeMayorEloConElDeMenor()
    {
        var jugador1 = CrearJugador(1, 1300);
        var jugador2 = CrearJugador(2, 1500);
        var jugador3 = CrearJugador(3, 1200);
        var jugador4 = CrearJugador(4, 1400);

        var pares = _estrategia.Emparejar([jugador1, jugador2, jugador3, jugador4]);

        Assert.AreSame(jugador2, pares[0].Jugador1);
        Assert.AreSame(jugador3, pares[0].Jugador2);
        Assert.AreSame(jugador4, pares[1].Jugador1);
        Assert.AreSame(jugador1, pares[1].Jugador2);
    }

    [TestMethod]
    public void Emparejar_CantidadImpar_LanzaInvalidOperation()
    {
        List<Usuario> jugadores = [CrearJugador(1), CrearJugador(2), CrearJugador(3)];

        Assert.ThrowsExactly<InvalidOperationException>(() => _estrategia.Emparejar(jugadores));
    }

    private static Usuario CrearJugador(int id, int elo = 1200)
    {
        var jugador = new Usuario("Ana", "Pérez", $"jugador{id}@farmeort.com", $"jugador{id}", new DateOnly(2000, 1, 15), Rol.Jugador, "hash", FechaActual)
        {
            Id = id,
        };
        jugador.ActualizarElo(elo);
        return jugador;
    }
}
