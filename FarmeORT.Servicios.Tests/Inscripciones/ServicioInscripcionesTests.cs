using FarmeORT.Dominio.Inscripciones;
using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.DTOs.Inscripciones;
using FarmeORT.DTOs.Usuarios;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Inscripciones;
using FarmeORT.IRepositorios.Torneos;
using FarmeORT.IRepositorios.Usuarios;
using FarmeORT.Servicios.Inscripciones;
using Moq;

namespace FarmeORT.Servicios.Tests.Inscripciones;

[TestClass]
public sealed class ServicioInscripcionesTests
{
    private const int TorneoId = 4;
    private const int JugadorId = 7;

    private static readonly DateOnly FechaActual = new(2026, 10, 1);
    private static readonly DateTime Ahora = new(2026, 10, 1, 12, 30, 0);

    private Mock<IRepositorioInscripciones> _repositorioInscripciones = null!;
    private Mock<IRepositorioTorneos> _repositorioTorneos = null!;
    private Mock<IRepositorioUsuarios> _repositorioUsuarios = null!;
    private Mock<TimeProvider> _reloj = null!;
    private Torneo _torneo = null!;
    private Usuario _jugador = null!;
    private ServicioInscripciones _servicio = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _repositorioInscripciones = new Mock<IRepositorioInscripciones>(MockBehavior.Strict);
        _repositorioTorneos = new Mock<IRepositorioTorneos>(MockBehavior.Strict);
        _repositorioUsuarios = new Mock<IRepositorioUsuarios>(MockBehavior.Strict);
        _reloj = new Mock<TimeProvider>(MockBehavior.Strict);
        var organizador = new Usuario("Laura", "Gomez", "laura@farmeort.com", "laurag", new DateOnly(1990, 5, 20), Rol.Organizador, "hash", FechaActual)
        {
            Id = 2,
        };
        _torneo = new Torneo("COPAAURA", "Copa Aura", "Torneo eliminatorio de batallas de aura.", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30), 4, organizador, FechaActual)
        {
            Id = TorneoId,
        };
        _jugador = new Usuario("Ana", "Pérez", "ana@farmeort.com", "anaperez", new DateOnly(2000, 1, 15), Rol.Jugador, "hash", FechaActual)
        {
            Id = JugadorId,
        };
        _servicio = new ServicioInscripciones(
            _repositorioInscripciones.Object,
            _repositorioTorneos.Object,
            _repositorioUsuarios.Object,
            _reloj.Object);
    }

    [TestMethod]
    public void Inscribir_TorneoAbiertoSinObligatorias_GuardaLaInscripcionAceptadaYLaDevuelve()
    {
        ConfigurarInscripcionValida();

        var response = _servicio.Inscribir(JugadorId, new InscripcionRequest { TorneoId = TorneoId });

        _repositorioInscripciones.Verify(
            r => r.Agregar(It.Is<Inscripcion>(i =>
                i.Torneo == _torneo
                && i.TorneoId == TorneoId
                && i.Jugador == _jugador
                && i.JugadorId == JugadorId
                && i.Estado == EstadoInscripcion.Aceptada
                && i.Fecha == Ahora)),
            Times.Once);
        Assert.AreEqual(JugadorId, response.JugadorId);
        Assert.AreEqual("anaperez", response.Nickname);
        Assert.AreEqual(1200, response.Elo);
        Assert.AreEqual(TorneoId, response.TorneoId);
        Assert.AreEqual("Copa Aura", response.Torneo);
        Assert.AreEqual(nameof(EstadoInscripcion.Aceptada), response.Estado);
        Assert.AreEqual(Ahora, response.Fecha);
    }

    [TestMethod]
    public void Inscribir_TorneoInexistente_LanzaTorneoNoEncontradoSinGuardar()
    {
        _repositorioTorneos.Setup(r => r.ObtenerPorId(99)).Returns((Torneo?)null);

        Assert.ThrowsExactly<TorneoNoEncontradoException>(() => _servicio.Inscribir(JugadorId, new InscripcionRequest { TorneoId = 99 }));
        _repositorioInscripciones.Verify(r => r.Agregar(It.IsAny<Inscripcion>()), Times.Never);
    }

    [TestMethod]
    public void Inscribir_SinTorneo_LanzaDatosInvalidosSinConsultarLaBase()
    {
        var excepcion = Assert.ThrowsExactly<DatosInvalidosException>(() => _servicio.Inscribir(JugadorId, new InscripcionRequest()));

        Assert.AreEqual("El torneo es obligatorio.", excepcion.Message);
    }

    [TestMethod]
    public void Inscribir_JugadorInexistente_LanzaUsuarioNoEncontradoSinGuardar()
    {
        _repositorioTorneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns(_torneo);
        _repositorioUsuarios.Setup(r => r.ObtenerConRepertorio(JugadorId)).Returns((Usuario?)null);

        Assert.ThrowsExactly<UsuarioNoEncontradoException>(() => _servicio.Inscribir(JugadorId, new InscripcionRequest { TorneoId = TorneoId }));
        _repositorioInscripciones.Verify(r => r.Agregar(It.IsAny<Inscripcion>()), Times.Never);
    }

    [TestMethod]
    public void Inscribir_JugadorYaInscriptoYAceptado_LanzaInscripcionYaExisteSinGuardar()
    {
        ConfigurarInscripcionValida();
        _repositorioInscripciones.Setup(r => r.ExisteAceptada(TorneoId, JugadorId)).Returns(true);

        Assert.ThrowsExactly<InscripcionYaExisteException>(() => _servicio.Inscribir(JugadorId, new InscripcionRequest { TorneoId = TorneoId }));
        _repositorioInscripciones.Verify(r => r.Agregar(It.IsAny<Inscripcion>()), Times.Never);
    }

    [TestMethod]
    public void Inscribir_CupoDeAceptadasCompleto_LanzaTorneoCompletoSinGuardar()
    {
        ConfigurarInscripcionValida();
        _repositorioInscripciones.Setup(r => r.ContarAceptadas(TorneoId)).Returns(4);

        Assert.ThrowsExactly<TorneoCompletoException>(() => _servicio.Inscribir(JugadorId, new InscripcionRequest { TorneoId = TorneoId }));
        _repositorioInscripciones.Verify(r => r.Agregar(It.IsAny<Inscripcion>()), Times.Never);
    }

    [TestMethod]
    public void Cancelar_InscripcionPropiaConTorneoAbierto_GuardaLaInscripcionCanceladaYLaDevuelve()
    {
        var inscripcion = CrearInscripcion();
        _repositorioInscripciones.Setup(r => r.ObtenerPorId(12)).Returns(inscripcion);
        _repositorioInscripciones.Setup(r => r.Actualizar(inscripcion));

        var response = _servicio.Cancelar(12, JugadorId);

        _repositorioInscripciones.Verify(r => r.Actualizar(It.Is<Inscripcion>(i => i == inscripcion && i.Estado == EstadoInscripcion.Cancelada)), Times.Once);
        Assert.AreEqual(12, response.Id);
        Assert.AreEqual(nameof(EstadoInscripcion.Cancelada), response.Estado);
    }

    [TestMethod]
    public void Cancelar_InscripcionInexistente_LanzaInscripcionNoEncontradaSinGuardar()
    {
        _repositorioInscripciones.Setup(r => r.ObtenerPorId(99)).Returns((Inscripcion?)null);

        Assert.ThrowsExactly<InscripcionNoEncontradaException>(() => _servicio.Cancelar(99, JugadorId));
        _repositorioInscripciones.Verify(r => r.Actualizar(It.IsAny<Inscripcion>()), Times.Never);
    }

    [TestMethod]
    public void Cancelar_InscripcionDeOtroJugador_LanzaNoEsDuenoDeLaInscripcionSinGuardar()
    {
        var inscripcion = CrearInscripcion();
        _repositorioInscripciones.Setup(r => r.ObtenerPorId(12)).Returns(inscripcion);

        Assert.ThrowsExactly<NoEsDuenoDeLaInscripcionException>(() => _servicio.Cancelar(12, 99));
        _repositorioInscripciones.Verify(r => r.Actualizar(It.IsAny<Inscripcion>()), Times.Never);
        Assert.AreEqual(EstadoInscripcion.Aceptada, inscripcion.Estado);
    }

    [TestMethod]
    public void Cancelar_TorneoQueYaNoEstaAbierto_LanzaTorneoNoAbiertoSinGuardar()
    {
        var inscripcion = CrearInscripcion();
        _torneo.Cancelar();
        _repositorioInscripciones.Setup(r => r.ObtenerPorId(12)).Returns(inscripcion);

        Assert.ThrowsExactly<TorneoNoAbiertoException>(() => _servicio.Cancelar(12, JugadorId));
        _repositorioInscripciones.Verify(r => r.Actualizar(It.IsAny<Inscripcion>()), Times.Never);
    }

    [TestMethod]
    public void ObtenerPorId_ConsultaElAdministrador_DevuelveLaInscripcion()
    {
        _repositorioInscripciones.Setup(r => r.ObtenerPorId(12)).Returns(CrearInscripcion());

        var response = _servicio.ObtenerPorId(12, 1, Roles.Administrador);

        Assert.AreEqual(12, response.Id);
        Assert.AreEqual("anaperez", response.Nickname);
    }

    [TestMethod]
    public void ObtenerPorId_JugadorConsultaUnaAjena_LanzaNoEsDuenoDeLaInscripcion()
    {
        _repositorioInscripciones.Setup(r => r.ObtenerPorId(12)).Returns(CrearInscripcion());

        Assert.ThrowsExactly<NoEsDuenoDeLaInscripcionException>(() => _servicio.ObtenerPorId(12, 99, Roles.Jugador));
    }

    [TestMethod]
    public void ObtenerPorId_OrganizadorDeOtroTorneo_LanzaNoEsOrganizadorDelTorneo()
    {
        _repositorioInscripciones.Setup(r => r.ObtenerPorId(12)).Returns(CrearInscripcion());

        Assert.ThrowsExactly<NoEsOrganizadorDelTorneoException>(() => _servicio.ObtenerPorId(12, 99, Roles.Organizador));
    }

    [TestMethod]
    [DataRow(JugadorId, Roles.Jugador)]
    [DataRow(2, Roles.Organizador)]
    public void ObtenerPorId_JugadorDuenoUOrganizadorDelTorneo_DevuelveLaInscripcion(int usuarioId, string rol)
    {
        _repositorioInscripciones.Setup(r => r.ObtenerPorId(12)).Returns(CrearInscripcion());

        var response = _servicio.ObtenerPorId(12, usuarioId, rol);

        Assert.AreEqual(12, response.Id);
    }

    [TestMethod]
    public void Listar_ConsultaElAdministrador_DevuelveLasInscripcionesConLosFiltrosDelPedido()
    {
        var desde = new DateOnly(2026, 10, 1);
        var hasta = new DateOnly(2026, 10, 31);
        var filtro = new FiltroInscripciones(TorneoId, EstadoInscripcion.Aceptada, desde, hasta, null, null, 10, 5);
        _repositorioInscripciones.Setup(r => r.Listar(filtro)).Returns([CrearInscripcion()]);
        var request = new FiltroInscripcionesRequest
        {
            TorneoId = TorneoId,
            Estado = nameof(EstadoInscripcion.Aceptada),
            FechaDesde = desde,
            FechaHasta = hasta,
            Limit = 10,
            Offset = 5,
        };

        var inscripciones = _servicio.Listar(request, 1, Roles.Administrador);

        Assert.HasCount(1, inscripciones);
        Assert.AreEqual(12, inscripciones[0].Id);
    }

    [TestMethod]
    public void Listar_ConsultaUnJugador_PideSoloSusInscripciones()
    {
        var filtro = new FiltroInscripciones(null, null, null, null, JugadorId, null, 20, 0);
        _repositorioInscripciones.Setup(r => r.Listar(filtro)).Returns([]);

        var inscripciones = _servicio.Listar(new FiltroInscripcionesRequest(), JugadorId, Roles.Jugador);

        Assert.IsEmpty(inscripciones);
    }

    [TestMethod]
    public void Listar_ConsultaUnOrganizador_PideSoloLasDeSusTorneos()
    {
        var filtro = new FiltroInscripciones(null, null, null, null, null, 2, 20, 0);
        _repositorioInscripciones.Setup(r => r.Listar(filtro)).Returns([]);

        var inscripciones = _servicio.Listar(new FiltroInscripcionesRequest(), 2, Roles.Organizador);

        Assert.IsEmpty(inscripciones);
    }

    [TestMethod]
    [DataRow("Pendiente")]
    [DataRow("aceptada")]
    [DataRow("1")]
    public void Listar_EstadoInexistente_NoFiltraPorEstado(string estado)
    {
        var filtro = new FiltroInscripciones(null, null, null, null, null, null, 20, 0);
        _repositorioInscripciones.Setup(r => r.Listar(filtro)).Returns([]);

        var inscripciones = _servicio.Listar(new FiltroInscripcionesRequest { Estado = estado }, 1, Roles.Administrador);

        Assert.IsEmpty(inscripciones);
    }

    [TestMethod]
    public void Listar_PaginacionFueraDeRango_LaAjusta()
    {
        var filtro = new FiltroInscripciones(null, null, null, null, null, null, 100, 0);
        _repositorioInscripciones.Setup(r => r.Listar(filtro)).Returns([]);

        var inscripciones = _servicio.Listar(new FiltroInscripcionesRequest { Limit = 600, Offset = -2 }, 1, Roles.Administrador);

        Assert.IsEmpty(inscripciones);
    }

    private void ConfigurarInscripcionValida()
    {
        _repositorioTorneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns(_torneo);
        _repositorioUsuarios.Setup(r => r.ObtenerConRepertorio(JugadorId)).Returns(_jugador);
        _repositorioInscripciones.Setup(r => r.ExisteAceptada(TorneoId, JugadorId)).Returns(false);
        _repositorioInscripciones.Setup(r => r.ContarAceptadas(TorneoId)).Returns(3);
        _repositorioInscripciones.Setup(r => r.Agregar(It.Is<Inscripcion>(i => i.TorneoId == TorneoId)));
        _reloj.Setup(r => r.GetUtcNow()).Returns(new DateTimeOffset(Ahora, TimeSpan.Zero));
        _reloj.Setup(r => r.LocalTimeZone).Returns(TimeZoneInfo.Utc);
    }

    private Inscripcion CrearInscripcion()
    {
        return new Inscripcion(_torneo, _jugador, Ahora)
        {
            Id = 12,
        };
    }
}
