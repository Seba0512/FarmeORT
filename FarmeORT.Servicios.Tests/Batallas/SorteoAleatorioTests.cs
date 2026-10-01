using FarmeORT.Dominio.Usuarios;
using FarmeORT.Servicios.Batallas;

namespace FarmeORT.Servicios.Tests.Batallas;

[TestClass]
public sealed class SorteoAleatorioTests
{
    [TestMethod]
    public void ElegirEntre_DosJugadores_DevuelveUnoDeLosParticipantes()
    {
        var fecha = new DateOnly(2026, 10, 1);
        var jugador1 = new Usuario("Ana", "Pérez", "ana@farmeort.com", "anaperez",
            new DateOnly(2000, 1, 1), Rol.Jugador, "hash", fecha)
        { Id = 7 };
        var jugador2 = new Usuario("Juan", "Pérez", "juan@farmeort.com", "juanperez",
            new DateOnly(2000, 1, 1), Rol.Jugador, "hash", fecha)
        { Id = 8 };
        var sorteo = new SorteoAleatorio();

        var elegido = sorteo.ElegirEntre(jugador1, jugador2);

        Assert.IsTrue(ReferenceEquals(elegido, jugador1) || ReferenceEquals(elegido, jugador2));
    }
}
