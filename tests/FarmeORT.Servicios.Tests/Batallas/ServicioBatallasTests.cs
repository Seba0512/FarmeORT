using FarmeORT.Dominio.Batallas;
using FarmeORT.Dominio.Skills;
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
    private const int BatallaId = 12;
    private const int TorneoId = 5;
    private const int OrganizadorId = 2;
    private const int OtroUsuarioId = 9;

    private static readonly DateOnly FechaActual = new(2026, 10, 2);
    private static readonly DateTimeOffset FechaPrimerLanzamiento = new(2026, 10, 2, 12, 0, 0, TimeSpan.Zero);

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
            FechaActual.AddDays(1), FechaActual.AddDays(2), 4, CrearUsuario(OrganizadorId, Rol.Organizador), FechaActual)
        { Id = TorneoId };
        var sorteo = new Mock<ISorteo>(MockBehavior.Strict);
        sorteo.Setup(s => s.ElegirEntre(jugador1, jugador2)).Returns(jugador1);
        _batalla = new Batalla(torneo, 1, jugador1, jugador2, sorteo.Object) { Id = BatallaId };
    }

    [TestMethod]
    [DataRow(Roles.Jugador, OtroUsuarioId)]
    [DataRow(Roles.Organizador, OrganizadorId)]
    [DataRow(Roles.Administrador, OtroUsuarioId)]
    public void ObtenerPorId_UsuarioAutorizado_DevuelveElFalloYElHistorial(string rol, int usuarioId)
    {
        PrepararBatallaFalladaConDosLanzamientos();

        var detalle = _servicio.ObtenerPorId(BatallaId, usuarioId, rol);

        Assert.AreEqual(BatallaId, detalle.Id);
        Assert.AreEqual(TorneoId, detalle.TorneoId);
        Assert.AreEqual(1, detalle.Ronda);
        Assert.AreEqual(7, detalle.Jugador1.Id);
        Assert.AreEqual(8, detalle.Jugador2.Id);
        Assert.AreEqual("Finalizada", detalle.Estado);
        Assert.AreEqual(8, detalle.GanadorId);
        Assert.AreEqual("Victoria por abandono.", detalle.Comentario);
        Assert.AreEqual(-16, detalle.CambioEloJugador1);
        Assert.AreEqual(16, detalle.CambioEloJugador2);
        Assert.IsNull(detalle.JugadorTurnoId);
        CollectionAssert.AreEqual(new[] { 1, 2 }, detalle.Lanzamientos.Select(l => l.Turno).ToArray());
    }

    [TestMethod]
    [DataRow(Roles.Jugador, OtroUsuarioId)]
    [DataRow(Roles.Organizador, OrganizadorId)]
    [DataRow(Roles.Administrador, OtroUsuarioId)]
    public void ListarLanzamientos_UsuarioAutorizado_DevuelveElHistorialOrdenado(string rol, int usuarioId)
    {
        PrepararBatallaFalladaConDosLanzamientos();

        var historial = _servicio.ListarLanzamientos(BatallaId, usuarioId, rol);

        CollectionAssert.AreEqual(new[] { 1, 2 }, historial.Select(l => l.Turno).ToArray());
        Assert.AreEqual(7, historial[0].JugadorId);
        Assert.AreEqual("usuario7", historial[0].Nickname);
        Assert.AreEqual(3, historial[0].SkillId);
        Assert.AreEqual("Six-Seven", historial[0].Skill);
        Assert.AreEqual(FechaPrimerLanzamiento, historial[0].Fecha);
        Assert.AreEqual(FechaPrimerLanzamiento.AddMinutes(1), historial[1].Fecha);
    }

    [TestMethod]
    public void ObtenerPorId_JugadorSinInscripcionEnElTorneo_LanzaJugadorNoInscriptoEnTorneo()
    {
        _batallas.Setup(r => r.ObtenerPorId(BatallaId)).Returns(_batalla);
        _inscripciones.Setup(r => r.ExisteDelJugador(TorneoId, OtroUsuarioId)).Returns(false);

        Assert.ThrowsExactly<JugadorNoInscriptoEnTorneoException>(() => _servicio.ObtenerPorId(BatallaId, OtroUsuarioId, Roles.Jugador));
    }

    [TestMethod]
    public void ObtenerPorId_OrganizadorDeOtroTorneo_LanzaNoEsOrganizadorDelTorneo()
    {
        _batallas.Setup(r => r.ObtenerPorId(BatallaId)).Returns(_batalla);

        Assert.ThrowsExactly<NoEsOrganizadorDelTorneoException>(() => _servicio.ObtenerPorId(BatallaId, OtroUsuarioId, Roles.Organizador));
    }

    [TestMethod]
    public void ListarLanzamientos_JugadorSinInscripcionEnElTorneo_LanzaJugadorNoInscriptoEnTorneo()
    {
        _batallas.Setup(r => r.ObtenerPorId(BatallaId)).Returns(_batalla);
        _inscripciones.Setup(r => r.ExisteDelJugador(TorneoId, OtroUsuarioId)).Returns(false);

        Assert.ThrowsExactly<JugadorNoInscriptoEnTorneoException>(() => _servicio.ListarLanzamientos(BatallaId, OtroUsuarioId, Roles.Jugador));
    }

    [TestMethod]
    public void ListarLanzamientos_OrganizadorDeOtroTorneo_LanzaNoEsOrganizadorDelTorneo()
    {
        _batallas.Setup(r => r.ObtenerPorId(BatallaId)).Returns(_batalla);

        Assert.ThrowsExactly<NoEsOrganizadorDelTorneoException>(() => _servicio.ListarLanzamientos(BatallaId, OtroUsuarioId, Roles.Organizador));
    }

    [TestMethod]
    public void ObtenerPorId_BatallaInexistente_LanzaBatallaNoEncontrada()
    {
        _batallas.Setup(r => r.ObtenerPorId(BatallaId)).Returns((Batalla?)null);

        Assert.ThrowsExactly<BatallaNoEncontradaException>(() => _servicio.ObtenerPorId(BatallaId, 7, Roles.Jugador));
    }

    [TestMethod]
    public void ListarLanzamientos_BatallaInexistente_LanzaBatallaNoEncontrada()
    {
        _batallas.Setup(r => r.ObtenerPorId(BatallaId)).Returns((Batalla?)null);

        Assert.ThrowsExactly<BatallaNoEncontradaException>(() => _servicio.ListarLanzamientos(BatallaId, 7, Roles.Jugador));
    }

    [TestMethod]
    [DataRow(Roles.Jugador, OtroUsuarioId)]
    [DataRow(Roles.Organizador, OrganizadorId)]
    [DataRow(Roles.Administrador, OtroUsuarioId)]
    public void Listar_UsuarioAutorizado_DevuelveElResumenDeLasBatallas(string rol, int usuarioId)
    {
        _inscripciones.Setup(r => r.ExisteDelJugador(TorneoId, OtroUsuarioId)).Returns(true);
        PrepararListado(new FiltroBatallas(TorneoId, null, null, null, 20, 0));
        _batalla.Fallar(7, "Victoria por abandono.", 16, -16);

        var resultado = _servicio.Listar(new FiltroBatallasRequest { TorneoId = TorneoId }, usuarioId, rol);

        var resumen = resultado.Single();
        Assert.AreEqual(BatallaId, resumen.Id);
        Assert.AreEqual(TorneoId, resumen.TorneoId);
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
    }

    [TestMethod]
    [DataRow("Pendiente", EstadoBatalla.Pendiente)]
    [DataRow("EnArbitraje", EstadoBatalla.EnArbitraje)]
    [DataRow("Finalizada", EstadoBatalla.Finalizada)]
    [DataRow("Cancelada", EstadoBatalla.Cancelada)]
    public void Listar_EstadoConNombreExacto_FiltraPorEseEstado(string estado, EstadoBatalla estadoEsperado)
    {
        var esperado = new FiltroBatallas(TorneoId, estadoEsperado, null, null, 20, 0);
        PrepararListado(esperado);

        _servicio.Listar(new FiltroBatallasRequest { TorneoId = TorneoId, Estado = estado }, OtroUsuarioId, Roles.Administrador);

        _batallas.Verify(r => r.Listar(esperado), Times.Once);
    }

    [TestMethod]
    [DataRow("1")]
    [DataRow("pendiente")]
    public void Listar_EstadoInexistente_NoFiltraPorEstado(string estado)
    {
        var esperado = new FiltroBatallas(TorneoId, null, null, null, 20, 0);
        PrepararListado(esperado);

        _servicio.Listar(new FiltroBatallasRequest { TorneoId = TorneoId, Estado = estado }, OtroUsuarioId, Roles.Administrador);

        _batallas.Verify(r => r.Listar(esperado), Times.Once);
    }

    [TestMethod]
    public void Listar_RondaYJugadorPositivos_FiltraPorEllos()
    {
        var esperado = new FiltroBatallas(TorneoId, null, 2, 8, 20, 0);
        PrepararListado(esperado);

        _servicio.Listar(new FiltroBatallasRequest { TorneoId = TorneoId, Ronda = 2, JugadorId = 8 }, OtroUsuarioId, Roles.Administrador);

        _batallas.Verify(r => r.Listar(esperado), Times.Once);
    }

    [TestMethod]
    [DataRow(0, -1)]
    [DataRow(-1, 0)]
    public void Listar_RondaYJugadorNoPositivos_NoFiltraPorEllos(int ronda, int jugadorId)
    {
        var esperado = new FiltroBatallas(TorneoId, null, null, null, 20, 0);
        PrepararListado(esperado);

        _servicio.Listar(new FiltroBatallasRequest { TorneoId = TorneoId, Ronda = ronda, JugadorId = jugadorId }, OtroUsuarioId, Roles.Administrador);

        _batallas.Verify(r => r.Listar(esperado), Times.Once);
    }

    [TestMethod]
    [DataRow(10, 3, 10, 3)]
    [DataRow(150, -3, 100, 0)]
    [DataRow(0, 0, 20, 0)]
    [DataRow(-5, -10, 20, 0)]
    public void Listar_Paginacion_AjustaLosValoresFueraDeRango(int limit, int offset, int limiteEsperado, int desplazamientoEsperado)
    {
        var esperado = new FiltroBatallas(TorneoId, null, null, null, limiteEsperado, desplazamientoEsperado);
        PrepararListado(esperado);

        _servicio.Listar(new FiltroBatallasRequest { TorneoId = TorneoId, Limit = limit, Offset = offset }, OtroUsuarioId, Roles.Administrador);

        _batallas.Verify(r => r.Listar(esperado), Times.Once);
    }

    [TestMethod]
    public void Listar_JugadorSinInscripcion_RechazaAunqueFiltroIncluyaOtroJugador()
    {
        _torneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns(_batalla.Torneo);
        _inscripciones.Setup(r => r.ExisteDelJugador(TorneoId, OtroUsuarioId)).Returns(false);
        var filtro = new FiltroBatallasRequest { TorneoId = TorneoId, JugadorId = 7 };

        Assert.ThrowsExactly<JugadorNoInscriptoEnTorneoException>(() => _servicio.Listar(filtro, OtroUsuarioId, Roles.Jugador));
    }

    [TestMethod]
    public void Listar_OrganizadorAjeno_RechazaAntesDeConsultarBatallas()
    {
        _torneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns(_batalla.Torneo);
        var filtro = new FiltroBatallasRequest { TorneoId = TorneoId };

        Assert.ThrowsExactly<NoEsOrganizadorDelTorneoException>(() => _servicio.Listar(filtro, 3, Roles.Organizador));
    }

    [TestMethod]
    public void Listar_TorneoInexistente_RechazaSinConsultarBatallas()
    {
        _torneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns((Torneo?)null);
        var filtro = new FiltroBatallasRequest { TorneoId = TorneoId };

        Assert.ThrowsExactly<TorneoNoEncontradoException>(() => _servicio.Listar(filtro, 7, Roles.Jugador));
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow(0)]
    [DataRow(-1)]
    public void Listar_TorneoIdInvalido_RechazaAntesDeConsultar(int? torneoId)
    {
        var filtro = new FiltroBatallasRequest { TorneoId = torneoId };

        Assert.ThrowsExactly<DatosInvalidosException>(() => _servicio.Listar(filtro, 7, Roles.Jugador));
    }

    // Batalla fallada por el jugador 8 con dos lanzamientos de la misma skill. El jugador consultante (9) está
    // inscripto en el torneo; el organizador y el administrador no necesitan esa consulta.
    private void PrepararBatallaFalladaConDosLanzamientos()
    {
        _batallas.Setup(r => r.ObtenerPorId(BatallaId)).Returns(_batalla);
        _inscripciones.Setup(r => r.ExisteDelJugador(TorneoId, OtroUsuarioId)).Returns(true);
        var skill = new Skill("SIXSEVEN", "Six-Seven", "Gesto alternando las manos hacia arriba y abajo.", CategoriaSkill.Gestual, 2) { Id = 3 };
        _batalla.Jugador1.DeclararSkill(skill, 4, FechaActual);
        _batalla.Jugador2.DeclararSkill(skill, 4, FechaActual);
        _batalla.Lanzar(_batalla.Jugador1, skill, FechaPrimerLanzamiento);
        _batalla.Lanzar(_batalla.Jugador2, skill, FechaPrimerLanzamiento.AddMinutes(1));
        _batalla.Fallar(8, "Victoria por abandono.", -16, 16);
    }

    private void PrepararListado(FiltroBatallas esperado)
    {
        _torneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns(_batalla.Torneo);
        _batallas.Setup(r => r.Listar(esperado)).Returns([_batalla]);
    }

    private static Usuario CrearUsuario(int id, Rol rol)
    {
        return new Usuario("Ana", "Pérez", $"usuario{id}@farmeort.com", $"usuario{id}",
            new DateOnly(2000, 1, 1), rol, "hash", FechaActual)
        { Id = id };
    }
}
