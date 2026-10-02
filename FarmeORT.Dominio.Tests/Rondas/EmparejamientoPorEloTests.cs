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

    private static Usuario CrearJugador(int id)
    {
        return new Usuario("Ana", "Pérez", $"jugador{id}@farmeort.com", $"jugador{id}", new DateOnly(2000, 1, 15), Rol.Jugador, "hash", FechaActual)
        {
            Id = id,
        };
    }
}
