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

    [TestMethod]
    public void Generar_OrganizadorNoAsignado_LanzaNoEsOrganizadorDelTorneo()
    {
        _repositorioTorneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns(_torneo);

        Assert.ThrowsExactly<NoEsOrganizadorDelTorneoException>(() => _servicio.Generar(TorneoId, 5, Roles.Organizador));
        Assert.AreEqual(EstadoTorneo.Abierto, _torneo.Estado);
        _repositorioBatallas.Verify(r => r.Agregar(It.IsAny<Batalla>()), Times.Never);
    }

    [TestMethod]
    public void Generar_AdministradorQueNoEsElOrganizador_GeneraLaRonda()
    {
        PrepararPrimeraRondaCompleta();

        var ronda = _servicio.Generar(TorneoId, 1, Roles.Administrador);

        Assert.AreEqual(1, ronda.Numero);
        Assert.AreEqual(EstadoTorneo.EnCurso, _torneo.Estado);
    }

    [TestMethod]
    public void Generar_TorneoIncompleto_NoCreaBatallasNiGuarda()
    {
        var inscripciones = _jugadores.Take(3).Select(j => new Inscripcion(_torneo, j, new DateTime(2026, 10, 2))).ToList();
        _repositorioTorneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns(_torneo);
        _repositorioInscripciones.Setup(r => r.ListarAceptadasDelTorneo(TorneoId)).Returns(inscripciones);

        Assert.ThrowsExactly<TorneoIncompletoException>(() => _servicio.Generar(TorneoId, OrganizadorId, Roles.Organizador));
        _repositorioBatallas.Verify(r => r.Agregar(It.IsAny<Batalla>()), Times.Never);
        _repositorioTorneos.Verify(r => r.Actualizar(It.IsAny<Torneo>()), Times.Never);
    }

    [TestMethod]
    public void Listar_TorneoConBatallas_AgrupaLasBatallasPorRonda()
    {
        IReadOnlyList<Batalla> batallas = [CrearBatalla(10, 1, 0, 3), CrearBatalla(11, 1, 1, 2), CrearBatalla(12, 2, 0, 1)];
        _repositorioTorneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns(_torneo);
        _repositorioBatallas.Setup(r => r.ListarDelTorneo(TorneoId)).Returns(batallas);

        var rondas = _servicio.Listar(TorneoId, false);

        Assert.HasCount(2, rondas);
        Assert.AreEqual(1, rondas[0].Numero);
        CollectionAssert.AreEqual(new[] { 10, 11 }, rondas[0].Batallas.Select(b => b.Id).ToArray());
        Assert.AreEqual(2, rondas[1].Numero);
        CollectionAssert.AreEqual(new[] { 12 }, rondas[1].Batallas.Select(b => b.Id).ToArray());
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void Listar_TorneoInexistente_LanzaTorneoNoEncontrado(bool esAnonimo)
    {
        _repositorioTorneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns((Torneo?)null);

        Assert.ThrowsExactly<TorneoNoEncontradoException>(() => _servicio.Listar(TorneoId, esAnonimo));
    }

    [TestMethod]
    public void Listar_AnonimoConTorneoCancelado_LanzaTorneoNoEncontrado()
    {
        _torneo.Cancelar();
        _repositorioTorneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns(_torneo);

        Assert.ThrowsExactly<TorneoNoEncontradoException>(() => _servicio.Listar(TorneoId, true));
    }

    [TestMethod]
    public void Listar_AutenticadoConTorneoCancelado_DevuelveSusRondas()
    {
        IReadOnlyList<Batalla> batallas = [CrearBatalla(10, 1, 0, 3)];
        _torneo.Cancelar();
        _repositorioTorneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns(_torneo);
        _repositorioBatallas.Setup(r => r.ListarDelTorneo(TorneoId)).Returns(batallas);

        var rondas = _servicio.Listar(TorneoId, false);

        Assert.HasCount(1, rondas);
    }

    [TestMethod]
    public void ObtenerPorNumero_RondaExistente_DevuelveSoloSusBatallas()
    {
        IReadOnlyList<Batalla> batallas = [CrearBatalla(10, 1, 0, 3), CrearBatalla(11, 1, 1, 2), CrearBatalla(12, 2, 0, 1)];
        _repositorioTorneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns(_torneo);
        _repositorioBatallas.Setup(r => r.ListarDelTorneo(TorneoId)).Returns(batallas);

        var ronda = _servicio.ObtenerPorNumero(TorneoId, 2, true);

        Assert.AreEqual(2, ronda.Numero);
        CollectionAssert.AreEqual(new[] { 12 }, ronda.Batallas.Select(b => b.Id).ToArray());
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(3)]
    public void ObtenerPorNumero_RondaInexistente_LanzaRondaNoEncontrada(int numero)
    {
        IReadOnlyList<Batalla> batallas = [CrearBatalla(10, 1, 0, 3), CrearBatalla(11, 1, 1, 2)];
        _repositorioTorneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns(_torneo);
        _repositorioBatallas.Setup(r => r.ListarDelTorneo(TorneoId)).Returns(batallas);

        Assert.ThrowsExactly<RondaNoEncontradaException>(() => _servicio.ObtenerPorNumero(TorneoId, numero, true));
    }

    [TestMethod]
    public void Listar_BatallaFallada_MuestraSuGanadorYQueEstaFinalizada()
    {
        var fallada = CrearBatalla(10, 1, 0, 3);
        fallada.Fallar(7, "Ganó por mejor ejecución de las skills.", 16, -16);
        IReadOnlyList<Batalla> batallas = [fallada];
        _repositorioTorneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns(_torneo);
        _repositorioBatallas.Setup(r => r.ListarDelTorneo(TorneoId)).Returns(batallas);

        var rondas = _servicio.Listar(TorneoId, true);

        Assert.AreEqual(7, rondas[0].Batallas[0].GanadorId);
        Assert.AreEqual("Finalizada", rondas[0].Batallas[0].Estado);
    }

    [TestMethod]
    public void Generar_TorneoEnCursoConRondaResuelta_EmparejaALosQueSiguenEnLaRondaSiguiente()
    {
        var ganadores = _jugadores.Take(2).Select(j => new Inscripcion(_torneo, j, new DateTime(2026, 10, 2))).ToList();
        _torneo.Iniciar(4);
        IReadOnlyList<Batalla> primeraRonda = [CrearBatallaFallada(10, 1, 0, 3), CrearBatallaFallada(11, 1, 1, 2)];
        IReadOnlyList<ParDeJugadores> pares = [new(_jugadores[0], _jugadores[1])];
        _repositorioTorneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns(_torneo);
        _repositorioBatallas.Setup(r => r.ListarDelTorneo(TorneoId)).Returns(primeraRonda);
        _repositorioInscripciones.Setup(r => r.ListarAceptadasDelTorneo(TorneoId)).Returns(ganadores);
        _estrategia.Setup(e => e.Emparejar(It.Is<IReadOnlyList<Usuario>>(j => j.SequenceEqual(_jugadores.Take(2))))).Returns(pares);
        _sorteo.Setup(s => s.ElegirEntre(_jugadores[0], _jugadores[1])).Returns(_jugadores[0]);
        _repositorioBatallas.Setup(r => r.Agregar(It.Is<Batalla>(b => b.NumeroRonda == 2 && b.Jugador1Id == 7 && b.Jugador2Id == 8)));
        _repositorioTorneos.Setup(r => r.Actualizar(_torneo));

        var ronda = _servicio.Generar(TorneoId, OrganizadorId, Roles.Organizador);

        Assert.AreEqual(2, ronda.Numero);
        Assert.HasCount(1, ronda.Batallas);
        _repositorioBatallas.Verify(r => r.Agregar(It.Is<Batalla>(b => b.NumeroRonda == 2 && b.Jugador1Id == 7 && b.Jugador2Id == 8)), Times.Once);
        _repositorioTorneos.Verify(r => r.Actualizar(_torneo), Times.Once);
    }

    [TestMethod]
    public void Generar_RondaAnteriorSinResolver_LanzaRondaSinResolver()
    {
        var siguen = _jugadores.Take(3).Select(j => new Inscripcion(_torneo, j, new DateTime(2026, 10, 2))).ToList();
        _torneo.Iniciar(4);
        IReadOnlyList<Batalla> primeraRonda = [CrearBatallaFallada(10, 1, 0, 3), CrearBatalla(11, 1, 1, 2)];
        _repositorioTorneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns(_torneo);
        _repositorioBatallas.Setup(r => r.ListarDelTorneo(TorneoId)).Returns(primeraRonda);
        _repositorioInscripciones.Setup(r => r.ListarAceptadasDelTorneo(TorneoId)).Returns(siguen);

        Assert.ThrowsExactly<RondaSinResolverException>(() => _servicio.Generar(TorneoId, OrganizadorId, Roles.Organizador));
        _repositorioBatallas.Verify(r => r.Agregar(It.IsAny<Batalla>()), Times.Never);
    }

    [TestMethod]
    public void Generar_UnSoloJugadorSinEliminar_LanzaTorneoListoParaFinalizar()
    {
        var campeon = new List<Inscripcion> { new(_torneo, _jugadores[0], new DateTime(2026, 10, 2)) };
        _torneo.Iniciar(4);
        IReadOnlyList<Batalla> batallas = [CrearBatallaFallada(10, 1, 0, 3), CrearBatallaFallada(11, 1, 1, 2), CrearBatallaFallada(12, 2, 0, 1)];
        _repositorioTorneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns(_torneo);
        _repositorioBatallas.Setup(r => r.ListarDelTorneo(TorneoId)).Returns(batallas);
        _repositorioInscripciones.Setup(r => r.ListarAceptadasDelTorneo(TorneoId)).Returns(campeon);

        Assert.ThrowsExactly<TorneoListoParaFinalizarException>(() => _servicio.Generar(TorneoId, OrganizadorId, Roles.Organizador));
        _repositorioBatallas.Verify(r => r.Agregar(It.IsAny<Batalla>()), Times.Never);
    }

    [TestMethod]
    public void Generar_RondaAnteriorConUnaBatallaCancelada_LaTomaComoResuelta()
    {
        var siguen = _jugadores.Take(2).Select(j => new Inscripcion(_torneo, j, new DateTime(2026, 10, 2))).ToList();
        _torneo.Iniciar(4);
        var cancelada = CrearBatalla(11, 1, 1, 2);
        cancelada.Cancelar();
        IReadOnlyList<Batalla> primeraRonda = [CrearBatallaFallada(10, 1, 0, 3), cancelada];
        IReadOnlyList<ParDeJugadores> pares = [new(_jugadores[0], _jugadores[1])];
        _repositorioTorneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns(_torneo);
        _repositorioBatallas.Setup(r => r.ListarDelTorneo(TorneoId)).Returns(primeraRonda);
        _repositorioInscripciones.Setup(r => r.ListarAceptadasDelTorneo(TorneoId)).Returns(siguen);
        _estrategia.Setup(e => e.Emparejar(It.Is<IReadOnlyList<Usuario>>(j => j.SequenceEqual(_jugadores.Take(2))))).Returns(pares);
        _sorteo.Setup(s => s.ElegirEntre(_jugadores[0], _jugadores[1])).Returns(_jugadores[0]);
        _repositorioBatallas.Setup(r => r.Agregar(It.Is<Batalla>(b => b.NumeroRonda == 2)));
        _repositorioTorneos.Setup(r => r.Actualizar(_torneo));

        var ronda = _servicio.Generar(TorneoId, OrganizadorId, Roles.Organizador);

        Assert.AreEqual(2, ronda.Numero);
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

    private Batalla CrearBatalla(int id, int numeroRonda, int indiceJugador1, int indiceJugador2)
    {
        var jugador1 = _jugadores[indiceJugador1];
        var jugador2 = _jugadores[indiceJugador2];
        return new Batalla(_torneo, numeroRonda, jugador1, jugador2, new SorteoInicial(jugador1)) { Id = id };
    }

    // Gana el primero de los dos: la ronda queda resuelta.
    private Batalla CrearBatallaFallada(int id, int numeroRonda, int indiceJugador1, int indiceJugador2)
    {
        var batalla = CrearBatalla(id, numeroRonda, indiceJugador1, indiceJugador2);
        batalla.Fallar(batalla.Jugador1Id, "Ganó por mejor ejecución de las skills.", 16, -16);
        return batalla;
    }

    private static Usuario CrearUsuario(int id, Rol rol)
    {
        return new Usuario("Ana", "Pérez", $"usuario{id}@farmeort.com", $"usuario{id}", new DateOnly(2000, 1, 15), rol, "hash", FechaActual)
        {
            Id = id,
        };
    }

    private sealed class SorteoInicial(Usuario elegido) : ISorteo
    {
        public Usuario ElegirEntre(Usuario jugador1, Usuario jugador2)
        {
            return elegido;
        }
    }
}
