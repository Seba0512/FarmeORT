using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.DTOs.Ranking;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Torneos;
using FarmeORT.IRepositorios.Usuarios;
using FarmeORT.Servicios.Ranking;
using Moq;

namespace FarmeORT.Servicios.Tests.Ranking;

[TestClass]
public sealed class ServicioRankingTests
{
    private static readonly DateOnly FechaActual = new(2026, 10, 1);

    private Mock<IRepositorioUsuarios> _repositorioUsuarios = null!;
    private Mock<IRepositorioTorneos> _repositorioTorneos = null!;
    private ServicioRanking _servicio = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _repositorioUsuarios = new Mock<IRepositorioUsuarios>(MockBehavior.Strict);
        _repositorioTorneos = new Mock<IRepositorioTorneos>(MockBehavior.Strict);
        _servicio = new ServicioRanking(_repositorioUsuarios.Object, _repositorioTorneos.Object);
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

    [TestMethod]
    [DataRow(101)]
    [DataRow(500)]
    public void Obtener_CantidadMayorAlTope_PideElTope(int cantidad)
    {
        _repositorioUsuarios.Setup(r => r.ListarRanking(100)).Returns([]);

        var ranking = _servicio.Obtener(new FiltroRankingRequest { Cantidad = cantidad });

        Assert.IsEmpty(ranking);
        _repositorioUsuarios.Verify(r => r.ListarRanking(100), Times.Once);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(-3)]
    public void Obtener_CantidadMenorAUno_PideElValorPorDefecto(int cantidad)
    {
        _repositorioUsuarios.Setup(r => r.ListarRanking(10)).Returns([]);

        var ranking = _servicio.Obtener(new FiltroRankingRequest { Cantidad = cantidad });

        Assert.IsEmpty(ranking);
        _repositorioUsuarios.Verify(r => r.ListarRanking(10), Times.Once);
    }

    [TestMethod]
    public void Obtener_TorneoInexistente_LanzaTorneoNoEncontradoSinConsultarElRanking()
    {
        _repositorioTorneos.Setup(r => r.ObtenerPorId(99)).Returns((Torneo?)null);
        _repositorioUsuarios.Setup(r => r.ListarRanking(10)).Returns([]);

        Assert.ThrowsExactly<TorneoNoEncontradoException>(() => _servicio.Obtener(new FiltroRankingRequest { TorneoId = 99 }));

        _repositorioUsuarios.Verify(r => r.ListarRanking(It.IsAny<int>()), Times.Never);
    }

    private static Usuario CrearJugador(int id, string nickname)
    {
        return new Usuario("Ana", "Pérez", $"{nickname}@farmeort.com", nickname, new DateOnly(2000, 1, 15), Rol.Jugador, "hash", FechaActual)
        {
            Id = id,
        };
    }
}
