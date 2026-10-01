using FarmeORT.Dominio.Usuarios;
using FarmeORT.DTOs.Ranking;
using FarmeORT.IRepositorios.Usuarios;
using FarmeORT.Servicios.Ranking;
using Moq;

namespace FarmeORT.Servicios.Tests.Ranking;

[TestClass]
public sealed class ServicioRankingTests
{
    private static readonly DateOnly FechaActual = new(2026, 10, 1);

    private Mock<IRepositorioUsuarios> _repositorioUsuarios = null!;
    private ServicioRanking _servicio = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _repositorioUsuarios = new Mock<IRepositorioUsuarios>(MockBehavior.Strict);
        _servicio = new ServicioRanking(_repositorioUsuarios.Object);
    }

    [TestMethod]
    public void Obtener_CantidadValida_DevuelveLosJugadoresConSuPosicion()
    {
        IReadOnlyList<Usuario> jugadores = [CrearJugador(4, "aurafarmer"), CrearJugador(2, "sixseven")];
        _repositorioUsuarios.Setup(r => r.ListarRanking(5)).Returns(jugadores);

        var ranking = _servicio.Obtener(new FiltroRankingRequest { Cantidad = 5 });

        Assert.HasCount(2, ranking);
        Assert.AreEqual(1, ranking[0].Posicion);
        Assert.AreEqual("aurafarmer", ranking[0].Nickname);
        Assert.AreEqual(1200, ranking[0].Elo);
        Assert.AreEqual(0, ranking[0].BatallasGanadas);
        Assert.AreEqual(0, ranking[0].BatallasPerdidas);
        Assert.AreEqual(2, ranking[1].Posicion);
        Assert.AreEqual("sixseven", ranking[1].Nickname);
    }

    private static Usuario CrearJugador(int id, string nickname)
    {
        return new Usuario("Ana", "Pérez", $"{nickname}@farmeort.com", nickname, new DateOnly(2000, 1, 15), Rol.Jugador, "hash", FechaActual)
        {
            Id = id,
        };
    }
}
