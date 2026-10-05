using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.DTOs.Ranking;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Batallas;
using FarmeORT.IRepositorios.Torneos;
using FarmeORT.IRepositorios.Usuarios;
using FarmeORT.Servicios.Ranking;
using Moq;

namespace FarmeORT.Servicios.Tests.Ranking;

[TestClass]
public sealed class ServicioRankingTests
{
    private const int TorneoId = 4;

    private static readonly DateOnly FechaActual = new(2026, 10, 1);

    private Mock<IRepositorioUsuarios> _repositorioUsuarios = null!;
    private Mock<IRepositorioTorneos> _repositorioTorneos = null!;
    private Mock<IRepositorioBatallas> _repositorioBatallas = null!;
    private ServicioRanking _servicio = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _repositorioUsuarios = new Mock<IRepositorioUsuarios>(MockBehavior.Strict);
        _repositorioTorneos = new Mock<IRepositorioTorneos>(MockBehavior.Strict);
        _repositorioBatallas = new Mock<IRepositorioBatallas>(MockBehavior.Strict);
        _servicio = new ServicioRanking(_repositorioUsuarios.Object, _repositorioTorneos.Object, _repositorioBatallas.Object);
    }

    [TestMethod]
    public void Obtener_CantidadValida_DevuelveLosJugadoresConSuPosicionYSusBatallas()
    {
        IReadOnlyList<Usuario> jugadores = [CrearJugador(4, "aurafarmer"), CrearJugador(2, "sixseven")];
        _repositorioUsuarios.Setup(r => r.ListarRanking(5)).Returns(jugadores);

        // sixseven no tiene batallas Finalizadas: el repositorio no lo devuelve y el ranking muestra 0.
        _repositorioBatallas.Setup(r => r.ContarResultadosPorJugador(new[] { 4, 2 }, null))
            .Returns(new Dictionary<int, ResultadosDeBatallas> { [4] = new(3, 1) });

        var ranking = _servicio.Obtener(new FiltroRankingRequest { Cantidad = 5 });

        Assert.HasCount(2, ranking);
        Assert.AreEqual(1, ranking[0].Posicion);
        Assert.AreEqual("aurafarmer", ranking[0].Nickname);
        Assert.AreEqual(1200, ranking[0].Elo);
        Assert.AreEqual(3, ranking[0].BatallasGanadas);
        Assert.AreEqual(1, ranking[0].BatallasPerdidas);
        Assert.AreEqual(2, ranking[1].Posicion);
        Assert.AreEqual("sixseven", ranking[1].Nickname);
        Assert.AreEqual(0, ranking[1].BatallasGanadas);
        Assert.AreEqual(0, ranking[1].BatallasPerdidas);
    }

    [TestMethod]
    [DataRow(101)]
    [DataRow(500)]
    public void Obtener_CantidadMayorAlTope_PideElTope(int cantidad)
    {
        _repositorioUsuarios.Setup(r => r.ListarRanking(100)).Returns([]);

        _servicio.Obtener(new FiltroRankingRequest { Cantidad = cantidad });

        _repositorioUsuarios.Verify(r => r.ListarRanking(100), Times.Once);
        _repositorioUsuarios.Verify(r => r.ListarRanking(100), Times.Once);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(-3)]
    public void Obtener_CantidadMenorAUno_PideElValorPorDefecto(int cantidad)
    {
        _repositorioUsuarios.Setup(r => r.ListarRanking(10)).Returns([]);

        _servicio.Obtener(new FiltroRankingRequest { Cantidad = cantidad });

        _repositorioUsuarios.Verify(r => r.ListarRanking(10), Times.Once);
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

    [TestMethod]
    public void Obtener_TorneoCancelado_LanzaTorneoNoEncontradoSinConsultarElRanking()
    {
        var torneo = CrearTorneo();
        torneo.Cancelar();
        _repositorioTorneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns(torneo);
        _repositorioUsuarios.Setup(r => r.ListarRanking(10)).Returns([]);

        Assert.ThrowsExactly<TorneoNoEncontradoException>(() => _servicio.Obtener(new FiltroRankingRequest { TorneoId = TorneoId }));

        _repositorioUsuarios.Verify(r => r.ListarRanking(It.IsAny<int>()), Times.Never);
    }

    [TestMethod]
    public void Obtener_TorneoExistente_DevuelveElRankingDeSusParticipantesConSuPosicion()
    {
        _repositorioTorneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns(CrearTorneo());
        _repositorioUsuarios.Setup(r => r.ListarRanking(5)).Returns([CrearJugador(9, "otrotorneo")]);
        _repositorioUsuarios.Setup(r => r.ListarRankingDelTorneo(TorneoId, 5)).Returns([CrearJugador(4, "aurafarmer"), CrearJugador(2, "sixseven")]);
        _repositorioBatallas.Setup(r => r.ContarResultadosPorJugador(new[] { 4, 2 }, TorneoId)).Returns(new Dictionary<int, ResultadosDeBatallas>());

        var ranking = _servicio.Obtener(new FiltroRankingRequest { Cantidad = 5, TorneoId = TorneoId });

        CollectionAssert.AreEqual(new[] { "aurafarmer", "sixseven" }, ranking.Select(j => j.Nickname).ToArray());
        CollectionAssert.AreEqual(new[] { 1, 2 }, ranking.Select(j => j.Posicion).ToArray());
    }

    // Foro (debate 06): con el filtro por torneo, las batallas ganadas y perdidas siguen siendo las globales.
    [TestMethod]
    public void Obtener_TorneoExistente_CuentaLasBatallasGlobalesDeCadaJugador()
    {
        _repositorioTorneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns(CrearTorneo());
        _repositorioUsuarios.Setup(r => r.ListarRankingDelTorneo(TorneoId, 10)).Returns([CrearJugador(4, "aurafarmer")]);
        _repositorioBatallas.Setup(r => r.ContarResultadosPorJugador(new[] { 4 }, TorneoId))
            .Returns(new Dictionary<int, ResultadosDeBatallas> { [4] = new(1, 0) });
        _repositorioBatallas.Setup(r => r.ContarResultadosPorJugador(new[] { 4 }, null))
            .Returns(new Dictionary<int, ResultadosDeBatallas> { [4] = new(7, 2) });

        var ranking = _servicio.Obtener(new FiltroRankingRequest { TorneoId = TorneoId });

        Assert.AreEqual(7, ranking[0].BatallasGanadas);
        Assert.AreEqual(2, ranking[0].BatallasPerdidas);
    }

    private static Torneo CrearTorneo()
    {
        var organizador = new Usuario("Laura", "Gomez", "laura@farmeort.com", "laurag", new DateOnly(1990, 5, 20), Rol.Organizador, "hash", FechaActual)
        {
            Id = 2,
        };
        return new Torneo("COPAAURA", "Copa Aura", "Torneo eliminatorio de batallas de aura.",
            FechaActual.AddDays(1), FechaActual.AddDays(30), 8, organizador, FechaActual)
        {
            Id = TorneoId,
        };
    }

    private static Usuario CrearJugador(int id, string nickname)
    {
        return new Usuario("Ana", "Pérez", $"{nickname}@farmeort.com", nickname, new DateOnly(2000, 1, 15), Rol.Jugador, "hash", FechaActual)
        {
            Id = id,
        };
    }
}
