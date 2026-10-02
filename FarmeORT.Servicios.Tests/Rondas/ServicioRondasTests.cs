using FarmeORT.Dominio.Batallas;
using FarmeORT.Dominio.Inscripciones;
using FarmeORT.Dominio.Rondas;
using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.DTOs.Usuarios;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Batallas;
using FarmeORT.IRepositorios.Inscripciones;
using FarmeORT.IRepositorios.Torneos;
using FarmeORT.Servicios.Rondas;
using Moq;

namespace FarmeORT.Servicios.Tests.Rondas;

[TestClass]
public sealed class ServicioRondasTests
{
    private const int TorneoId = 4;
    private const int OrganizadorId = 2;

    private static readonly DateOnly FechaActual = new(2026, 10, 2);

    private Mock<IRepositorioTorneos> _repositorioTorneos = null!;
    private Mock<IRepositorioInscripciones> _repositorioInscripciones = null!;
    private Mock<IRepositorioBatallas> _repositorioBatallas = null!;
    private Mock<IEstrategiaEmparejamiento> _estrategia = null!;
    private Mock<ISorteo> _sorteo = null!;
    private Torneo _torneo = null!;
    private List<Usuario> _jugadores = null!;
    private ServicioRondas _servicio = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _repositorioTorneos = new Mock<IRepositorioTorneos>(MockBehavior.Strict);
        _repositorioInscripciones = new Mock<IRepositorioInscripciones>(MockBehavior.Strict);
        _repositorioBatallas = new Mock<IRepositorioBatallas>(MockBehavior.Strict);
        _estrategia = new Mock<IEstrategiaEmparejamiento>(MockBehavior.Strict);
        _sorteo = new Mock<ISorteo>(MockBehavior.Strict);
        var organizador = CrearUsuario(OrganizadorId, Rol.Organizador);
        _torneo = new Torneo("COPAAURA", "Copa Aura", "Torneo eliminatorio de batallas de aura.", FechaActual, FechaActual.AddDays(10), 4, organizador, FechaActual)
        {
            Id = TorneoId,
        };
        _jugadores = [CrearUsuario(7, Rol.Jugador), CrearUsuario(8, Rol.Jugador), CrearUsuario(9, Rol.Jugador), CrearUsuario(10, Rol.Jugador)];
        _servicio = new ServicioRondas(_repositorioTorneos.Object, _repositorioInscripciones.Object, _repositorioBatallas.Object, _estrategia.Object, _sorteo.Object);
    }

    [TestMethod]
    public void Generar_PrimeraRonda_CreaUnaBatallaPorParYLaDevuelve()
    {
        PrepararPrimeraRondaCompleta();

        var ronda = _servicio.Generar(TorneoId, OrganizadorId, Roles.Organizador);

        Assert.AreEqual(1, ronda.Numero);
        Assert.HasCount(2, ronda.Batallas);
        Assert.AreEqual(7, ronda.Batallas[0].Jugador1.Id);
        Assert.AreEqual(10, ronda.Batallas[0].Jugador2.Id);
        Assert.AreEqual("Pendiente", ronda.Batallas[0].Estado);
        Assert.IsNull(ronda.Batallas[0].GanadorId);
        Assert.AreEqual(8, ronda.Batallas[1].Jugador1.Id);
        Assert.AreEqual(9, ronda.Batallas[1].Jugador2.Id);
        _repositorioBatallas.Verify(r => r.Agregar(It.Is<Batalla>(b => b.NumeroRonda == 1 && b.Jugador1Id == 7 && b.Jugador2Id == 10)), Times.Once);
        _repositorioBatallas.Verify(r => r.Agregar(It.Is<Batalla>(b => b.NumeroRonda == 1 && b.Jugador1Id == 8 && b.Jugador2Id == 9)), Times.Once);
    }

    [TestMethod]
    public void Generar_PrimeraRonda_PasaElTorneoAEnCursoYGuardaTodoJunto()
    {
        PrepararPrimeraRondaCompleta();

        _servicio.Generar(TorneoId, OrganizadorId, Roles.Organizador);

        Assert.AreEqual(EstadoTorneo.EnCurso, _torneo.Estado);
        _repositorioTorneos.Verify(r => r.Actualizar(_torneo), Times.Once);
    }

    [TestMethod]
    public void Generar_TorneoInexistente_LanzaTorneoNoEncontrado()
    {
        _repositorioTorneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns((Torneo?)null);

        Assert.ThrowsExactly<TorneoNoEncontradoException>(() => _servicio.Generar(TorneoId, OrganizadorId, Roles.Organizador));
        _repositorioBatallas.Verify(r => r.Agregar(It.IsAny<Batalla>()), Times.Never);
    }

    // Torneo Abierto con los cuatro aceptados que pide el cupo; la estrategia cruza 7-10 y 8-9 y, como todos
    // tienen el mismo ELO, el sorteo elige al primero de cada par para empezar. Todo se guarda con el torneo.
    private void PrepararPrimeraRondaCompleta()
    {
        var inscripciones = _jugadores.Select(j => new Inscripcion(_torneo, j, new DateTime(2026, 10, 2))).ToList();
        IReadOnlyList<ParDeJugadores> pares = [new(_jugadores[0], _jugadores[3]), new(_jugadores[1], _jugadores[2])];
        _repositorioTorneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns(_torneo);
        _repositorioInscripciones.Setup(r => r.ListarAceptadasDelTorneo(TorneoId)).Returns(inscripciones);
        _estrategia.Setup(e => e.Emparejar(It.Is<IReadOnlyList<Usuario>>(j => j.SequenceEqual(_jugadores)))).Returns(pares);
        _sorteo.Setup(s => s.ElegirEntre(_jugadores[0], _jugadores[3])).Returns(_jugadores[0]);
        _sorteo.Setup(s => s.ElegirEntre(_jugadores[1], _jugadores[2])).Returns(_jugadores[1]);
        _repositorioBatallas.Setup(r => r.Agregar(It.Is<Batalla>(b => b.Jugador1Id == 7 && b.Jugador2Id == 10)));
        _repositorioBatallas.Setup(r => r.Agregar(It.Is<Batalla>(b => b.Jugador1Id == 8 && b.Jugador2Id == 9)));
        _repositorioTorneos.Setup(r => r.Actualizar(_torneo));
    }

    private static Usuario CrearUsuario(int id, Rol rol)
    {
        return new Usuario("Ana", "Pérez", $"usuario{id}@farmeort.com", $"usuario{id}", new DateOnly(2000, 1, 15), rol, "hash", FechaActual)
        {
            Id = id,
        };
    }
}
