using FarmeORT.Dominio.Batallas;
using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.DTOs.Batallas;
using FarmeORT.DTOs.Usuarios;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Batallas;
using FarmeORT.IRepositorios.Inscripciones;
using FarmeORT.IRepositorios.Torneos;
using FarmeORT.Servicios.Batallas;
using Moq;

namespace FarmeORT.Servicios.Tests.Batallas;

[TestClass]
public sealed class ServicioBatallasTests
{
    private static readonly DateOnly FechaActual = new(2026, 10, 2);

    private Mock<IRepositorioBatallas> _batallas = null!;
    private Mock<IRepositorioTorneos> _torneos = null!;
    private Mock<IRepositorioInscripciones> _inscripciones = null!;
    private ServicioBatallas _servicio = null!;
    private Batalla _batalla = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _batallas = new Mock<IRepositorioBatallas>(MockBehavior.Strict);
        _torneos = new Mock<IRepositorioTorneos>(MockBehavior.Strict);
        _inscripciones = new Mock<IRepositorioInscripciones>(MockBehavior.Strict);
        _servicio = new ServicioBatallas(_batallas.Object, _torneos.Object, _inscripciones.Object);
        var jugador1 = CrearUsuario(7, Rol.Jugador);
        var jugador2 = CrearUsuario(8, Rol.Jugador);
        var torneo = new Torneo("COPAAURA", "Copa Aura", "Torneo eliminatorio de batallas de aura.",
            FechaActual.AddDays(1), FechaActual.AddDays(2), 4, CrearUsuario(2, Rol.Organizador), FechaActual)
        { Id = 5 };
        var sorteo = new Mock<ISorteo>(MockBehavior.Strict);
        sorteo.Setup(s => s.ElegirEntre(jugador1, jugador2)).Returns(jugador1);
        _batalla = new Batalla(torneo, 1, jugador1, jugador2, sorteo.Object) { Id = 12 };
    }

    [TestMethod]
    [DataRow(Roles.Jugador, "Finalizada", 2, 8, 10, 3, EstadoBatalla.Finalizada, 2, 8, 10, 3)]
    [DataRow(Roles.Organizador, "EnArbitraje", 1, 7, 150, -3, EstadoBatalla.EnArbitraje, 1, 7, 100, 0)]
    [DataRow(Roles.Administrador, "1", 0, -1, 0, 0, null, null, null, 20, 0)]
    [DataRow(Roles.Administrador, null, null, null, 20, 0, null, null, null, 20, 0)]
    [DataRow(Roles.Administrador, "pendiente", -1, 0, -5, -10, null, null, null, 20, 0)]
    [DataRow(Roles.Administrador, "Cancelada", null, null, 1, 0, EstadoBatalla.Cancelada, null, null, 1, 0)]
    [DataRow(Roles.Administrador, "Pendiente", null, null, 1, 0, EstadoBatalla.Pendiente, null, null, 1, 0)]
    public void Listar_UsuarioAutorizado_NormalizaFiltrosYDevuelveResumen(
        string rol, string? estado, int? ronda, int? jugadorId, int limit, int offset,
        EstadoBatalla? estadoEsperado, int? rondaEsperada, int? jugadorEsperado, int limiteEsperado, int desplazamientoEsperado)
    {
        var usuarioId = rol == Roles.Organizador ? 2 : 9;
        _torneos.Setup(r => r.ObtenerPorId(5)).Returns(_batalla.Torneo);
        if(rol == Roles.Jugador)
        {
            _inscripciones.Setup(r => r.ExisteDelJugador(5, usuarioId)).Returns(true);
        }

        var esperado = new FiltroBatallas(5, estadoEsperado, rondaEsperada, jugadorEsperado, limiteEsperado, desplazamientoEsperado);
        _batallas.Setup(r => r.Listar(esperado)).Returns([_batalla]);
        _batalla.Fallar(7, "Victoria por abandono.", 16, -16);
        var filtro = new FiltroBatallasRequest { TorneoId = 5, Estado = estado, Ronda = ronda, JugadorId = jugadorId, Limit = limit, Offset = offset };

        var resultado = _servicio.Listar(filtro, usuarioId, rol);

        var resumen = resultado.Single();
        Assert.AreEqual(12, resumen.Id);
        Assert.AreEqual(5, resumen.TorneoId);
        Assert.AreEqual(1, resumen.Ronda);
        Assert.AreEqual(7, resumen.Jugador1.Id);
        Assert.AreEqual("usuario7", resumen.Jugador1.Nickname);
        Assert.AreEqual(1200, resumen.Jugador1.Elo);
        Assert.AreEqual(8, resumen.Jugador2.Id);
        Assert.AreEqual("usuario8", resumen.Jugador2.Nickname);
        Assert.AreEqual(1200, resumen.Jugador2.Elo);
        Assert.AreEqual("Finalizada", resumen.Estado);
        Assert.AreEqual(7, resumen.GanadorId);
        Assert.IsNull(resumen.JugadorTurnoId);
        _batallas.Verify(r => r.Listar(esperado), Times.Once);
        _batallas.Verify(r => r.Guardar(), Times.Never);
    }

    [TestMethod]
    public void Listar_JugadorSinInscripcion_RechazaAunqueFiltroIncluyaOtroJugador()
    {
        _torneos.Setup(r => r.ObtenerPorId(5)).Returns(_batalla.Torneo);
        _inscripciones.Setup(r => r.ExisteDelJugador(5, 9)).Returns(false);

        Assert.ThrowsExactly<JugadorNoInscriptoEnTorneoException>(() =>
            _servicio.Listar(new FiltroBatallasRequest { TorneoId = 5, JugadorId = 7 }, 9, Roles.Jugador));

        _inscripciones.Verify(r => r.ExisteDelJugador(5, 9), Times.Once);
        _batallas.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void Listar_OrganizadorAjeno_RechazaAntesDeConsultarBatallas()
    {
        _torneos.Setup(r => r.ObtenerPorId(5)).Returns(_batalla.Torneo);

        Assert.ThrowsExactly<NoEsOrganizadorDelTorneoException>(() =>
            _servicio.Listar(new FiltroBatallasRequest { TorneoId = 5 }, 3, Roles.Organizador));

        _batallas.VerifyNoOtherCalls();
        _inscripciones.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void Listar_TorneoInexistente_RechazaSinConsultarBatallas()
    {
        _torneos.Setup(r => r.ObtenerPorId(5)).Returns((Torneo?)null);

        Assert.ThrowsExactly<TorneoNoEncontradoException>(() =>
            _servicio.Listar(new FiltroBatallasRequest { TorneoId = 5 }, 7, Roles.Jugador));

        _torneos.Verify(r => r.ObtenerPorId(5), Times.Once);
        _batallas.VerifyNoOtherCalls();
        _inscripciones.VerifyNoOtherCalls();
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow(0)]
    [DataRow(-1)]
    public void Listar_TorneoIdInvalido_RechazaAntesDeConsultar(int? torneoId)
    {
        Assert.ThrowsExactly<DatosInvalidosException>(() =>
            _servicio.Listar(new FiltroBatallasRequest { TorneoId = torneoId }, 7, Roles.Jugador));
        _torneos.VerifyNoOtherCalls();
        _batallas.VerifyNoOtherCalls();
        _inscripciones.VerifyNoOtherCalls();
    }

    private static Usuario CrearUsuario(int id, Rol rol)
    {
        return new Usuario("Ana", "Pérez", $"usuario{id}@farmeort.com", $"usuario{id}",
            new DateOnly(2000, 1, 1), rol, "hash", FechaActual)
        { Id = id };
    }
}
