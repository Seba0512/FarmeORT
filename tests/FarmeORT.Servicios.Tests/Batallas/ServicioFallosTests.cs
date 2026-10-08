using FarmeORT.Dominio.Batallas;
using FarmeORT.Dominio.Elo;
using FarmeORT.Dominio.Inscripciones;
using FarmeORT.Dominio.Skills;
using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.DTOs.Batallas;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Batallas;
using FarmeORT.IRepositorios.Inscripciones;
using FarmeORT.Servicios.Batallas;
using Moq;

namespace FarmeORT.Servicios.Tests.Batallas;

[TestClass]
public sealed class ServicioFallosTests
{
    private const int BatallaId = 12;
    private const int TorneoId = 5;
    private const int OrganizadorId = 2;
    private const int IdJugador1 = 7;
    private const int IdJugador2 = 8;
    private const string Comentario = "Victoria por abandono.";
    private const int EloPrevioGanador = 1500;
    private const int EloPrevioPerdedor = 1100;
    private const int EloNuevoGanador = 1530;
    private const int EloNuevoPerdedor = 1080;

    private static readonly DateOnly FechaActual = new(2026, 10, 2);

    private Mock<IRepositorioBatallas> _batallas = null!;
    private Mock<IRepositorioInscripciones> _inscripciones = null!;
    private Mock<ICalculadorElo> _elo = null!;
    private ServicioFallos _servicio = null!;
    private Batalla _batalla = null!;
    private Inscripcion _inscripcion = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _batallas = new Mock<IRepositorioBatallas>(MockBehavior.Strict);
        _inscripciones = new Mock<IRepositorioInscripciones>(MockBehavior.Strict);
        _elo = new Mock<ICalculadorElo>(MockBehavior.Strict);
        _servicio = new ServicioFallos(_batallas.Object, _inscripciones.Object, _elo.Object);
        var jugador1 = CrearUsuario(IdJugador1, Rol.Jugador);
        var jugador2 = CrearUsuario(IdJugador2, Rol.Jugador);
        var torneo = new Torneo("COPAAURA", "Copa Aura", "Torneo eliminatorio de batallas de aura.",
            FechaActual.AddDays(1), FechaActual.AddDays(2), 4, CrearUsuario(OrganizadorId, Rol.Organizador), FechaActual)
        { Id = TorneoId };
        var sorteo = new Mock<ISorteo>(MockBehavior.Strict);
        sorteo.Setup(s => s.ElegirEntre(jugador1, jugador2)).Returns(jugador1);
        _batalla = new Batalla(torneo, 1, jugador1, jugador2, sorteo.Object) { Id = BatallaId };
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow(0)]
    [DataRow(-1)]
    public void Fallar_GanadorAusenteONoPositivo_RechazaSinConsultar(int? ganadorId)
    {
        var request = new FalloRequest { GanadorId = ganadorId, Comentario = Comentario };

        Assert.ThrowsExactly<DatosInvalidosException>(() => _servicio.Fallar(BatallaId, OrganizadorId, request));

        _batallas.Verify(r => r.ObtenerPorId(It.IsAny<int>()), Times.Never);
    }

    [TestMethod]
    public void Fallar_BatallaInexistente_RechazaSinCalcularNiGuardar()
    {
        _batallas.Setup(r => r.ObtenerPorId(99)).Returns((Batalla?)null);

        Assert.ThrowsExactly<BatallaNoEncontradaException>(() => _servicio.Fallar(99, OrganizadorId, CrearRequest(IdJugador1)));

        _elo.Verify(e => e.Calcular(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        _batallas.Verify(r => r.Guardar(), Times.Never);
    }

    [TestMethod]
    public void Fallar_OrganizadorAjeno_RechazaSinEfectos()
    {
        PrepararFalloValido(IdJugador1);

        Assert.ThrowsExactly<NoEsOrganizadorDelTorneoException>(() => _servicio.Fallar(BatallaId, 3, CrearRequest(IdJugador1)));

        AssertSinEfectos(EstadoBatalla.Pendiente);
    }

    [TestMethod]
    [DataRow(EstadoBatalla.Finalizada)]
    [DataRow(EstadoBatalla.Cancelada)]
    public void Fallar_BatallaResuelta_RechazaSinEfectos(EstadoBatalla estado)
    {
        LlevarBatallaA(estado);
        PrepararFalloValido(IdJugador1);

        Assert.ThrowsExactly<BatallaNoFallableException>(() => _servicio.Fallar(BatallaId, OrganizadorId, CrearRequest(IdJugador1)));

        AssertSinEfectos(estado);
    }

    [TestMethod]
    public void Fallar_GanadorAjeno_RechazaSinEfectos()
    {
        PrepararFalloValido(IdJugador1);

        Assert.ThrowsExactly<JugadorNoParticipaEnBatallaException>(() => _servicio.Fallar(BatallaId, OrganizadorId, CrearRequest(9)));

        AssertSinEfectos(EstadoBatalla.Pendiente);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("                    ")]
    public void Fallar_ComentarioVacio_RechazaSinEfectos(string? comentario)
    {
        PrepararFalloValido(IdJugador1);
        var request = new FalloRequest { GanadorId = IdJugador1, Comentario = comentario };

        Assert.ThrowsExactly<DatosInvalidosException>(() => _servicio.Fallar(BatallaId, OrganizadorId, request));

        AssertSinEfectos(EstadoBatalla.Pendiente);
    }

    [TestMethod]
    [DataRow(9)]
    [DataRow(501)]
    public void Fallar_ComentarioFueraDeRango_RechazaSinEfectos(int largo)
    {
        PrepararFalloValido(IdJugador1);
        var request = new FalloRequest { GanadorId = IdJugador1, Comentario = new string('a', largo) };

        Assert.ThrowsExactly<DatosInvalidosException>(() => _servicio.Fallar(BatallaId, OrganizadorId, request));

        AssertSinEfectos(EstadoBatalla.Pendiente);
    }

    [TestMethod]
    public void Fallar_PerdedorSinInscripcionAceptada_RechazaSinEfectos()
    {
        PrepararFalloValido(IdJugador1);
        _inscripciones.Setup(r => r.ObtenerAceptada(TorneoId, IdJugador2)).Returns((Inscripcion?)null);

        Assert.ThrowsExactly<InscripcionNoActivaException>(() => _servicio.Fallar(BatallaId, OrganizadorId, CrearRequest(IdJugador1)));

        AssertSinEfectos(EstadoBatalla.Pendiente);
    }

    [TestMethod]
    [DataRow(IdJugador1)]
    [DataRow(IdJugador2)]
    public void Fallar_DatosValidos_CalculaElEloConLosValoresPreviosDeAmbos(int ganadorId)
    {
        PrepararFalloValido(ganadorId);

        _servicio.Fallar(BatallaId, OrganizadorId, CrearRequest(ganadorId));

        _elo.Verify(e => e.Calcular(EloPrevioGanador, EloPrevioPerdedor), Times.Once);
    }

    [TestMethod]
    [DataRow(IdJugador1, EloNuevoGanador, EloNuevoPerdedor, 30, -20)]
    [DataRow(IdJugador2, EloNuevoPerdedor, EloNuevoGanador, -20, 30)]
    public void Fallar_DatosValidos_ActualizaElEloDeAmbosYDevuelveElCambio(
        int ganadorId,
        int eloJugador1,
        int eloJugador2,
        int cambioJugador1,
        int cambioJugador2)
    {
        PrepararFalloValido(ganadorId);

        var resultado = _servicio.Fallar(BatallaId, OrganizadorId, CrearRequest(ganadorId));

        Assert.AreEqual(eloJugador1, _batalla.Jugador1.Elo);
        Assert.AreEqual(eloJugador2, _batalla.Jugador2.Elo);
        Assert.AreEqual(eloJugador1, resultado.Jugador1.Elo);
        Assert.AreEqual(eloJugador2, resultado.Jugador2.Elo);
        Assert.AreEqual(cambioJugador1, resultado.CambioEloJugador1);
        Assert.AreEqual(cambioJugador2, resultado.CambioEloJugador2);
    }

    [TestMethod]
    [DataRow(EstadoBatalla.Pendiente)]
    [DataRow(EstadoBatalla.EnArbitraje)]
    public void Fallar_BatallaSinResolver_LaFinalizaConGanadorYComentario(EstadoBatalla estado)
    {
        LlevarBatallaA(estado);
        PrepararFalloValido(IdJugador1);

        var resultado = _servicio.Fallar(BatallaId, OrganizadorId, CrearRequest(IdJugador1));

        Assert.AreEqual(EstadoBatalla.Finalizada, _batalla.Estado);
        Assert.AreEqual(IdJugador1, _batalla.GanadorId);
        Assert.AreSame(_batalla.Jugador1, _batalla.Ganador);
        Assert.AreEqual(Comentario, _batalla.Comentario);
        Assert.IsNull(_batalla.JugadorTurnoId);
        Assert.AreEqual(BatallaId, resultado.Id);
        Assert.AreEqual(TorneoId, resultado.TorneoId);
        Assert.AreEqual(1, resultado.Ronda);
        Assert.AreEqual(nameof(EstadoBatalla.Finalizada), resultado.Estado);
        Assert.AreEqual(IdJugador1, resultado.GanadorId);
        Assert.AreEqual(Comentario, resultado.Comentario);
        Assert.AreEqual("usuario7", resultado.Jugador1.Nickname);
        Assert.AreEqual("usuario8", resultado.Jugador2.Nickname);
        Assert.IsNull(resultado.JugadorTurnoId);
    }

    [TestMethod]
    public void Fallar_DatosValidos_EliminaLaInscripcionDelPerdedor()
    {
        PrepararFalloValido(IdJugador1);

        _servicio.Fallar(BatallaId, OrganizadorId, CrearRequest(IdJugador1));

        Assert.AreEqual(EstadoInscripcion.Eliminada, _inscripcion.Estado);
    }

    [TestMethod]
    public void Fallar_DatosValidos_GuardaTodosLosEfectosEnUnSoloGuardado()
    {
        PrepararFalloValido(IdJugador1);
        EstadoBatalla? estadoAlGuardar = null;
        EstadoInscripcion? inscripcionAlGuardar = null;
        int? eloGanadorAlGuardar = null;
        _batallas.Setup(r => r.Guardar()).Callback(() =>
        {
            estadoAlGuardar = _batalla.Estado;
            inscripcionAlGuardar = _inscripcion.Estado;
            eloGanadorAlGuardar = _batalla.Jugador1.Elo;
        });

        _servicio.Fallar(BatallaId, OrganizadorId, CrearRequest(IdJugador1));

        _batallas.Verify(r => r.Guardar(), Times.Once);
        _inscripciones.Verify(r => r.Actualizar(It.IsAny<Inscripcion>()), Times.Never);
        Assert.AreEqual(EstadoBatalla.Finalizada, estadoAlGuardar);
        Assert.AreEqual(EstadoInscripcion.Eliminada, inscripcionAlGuardar);
        Assert.AreEqual(EloNuevoGanador, eloGanadorAlGuardar);
    }

    [TestMethod]
    public void Fallar_BatallaConLanzamientos_DevuelveHistorialOrdenado()
    {
        PrepararFalloValido(IdJugador1);
        var skill = new Skill("SIXSEVEN", "Six-Seven", "Gesto alternando las manos hacia arriba y abajo.", CategoriaSkill.Gestual, 2) { Id = 3 };
        _batalla.Jugador1.DeclararSkill(skill, 4, FechaActual);
        _batalla.Jugador2.DeclararSkill(skill, 4, FechaActual);
        var fecha = new DateTimeOffset(2026, 10, 2, 16, 30, 0, TimeSpan.Zero);
        _batalla.Lanzar(_batalla.Jugador1, skill, fecha);
        _batalla.Lanzar(_batalla.Jugador2, skill, fecha.AddMinutes(1));

        var resultado = _servicio.Fallar(BatallaId, OrganizadorId, CrearRequest(IdJugador1));

        CollectionAssert.AreEqual(new[] { 1, 2 }, resultado.Lanzamientos.Select(l => l.Turno).ToArray());
        Assert.AreEqual(IdJugador1, resultado.Lanzamientos[0].JugadorId);
        Assert.AreEqual("usuario7", resultado.Lanzamientos[0].Nickname);
        Assert.AreEqual(3, resultado.Lanzamientos[0].SkillId);
        Assert.AreEqual("Six-Seven", resultado.Lanzamientos[0].Skill);
        Assert.AreEqual(fecha, resultado.Lanzamientos[0].Fecha);
    }

    // El ganador llega con 1500 de ELO y el perdedor con 1100; el calculador devuelve 1530 y 1080.
    private void PrepararFalloValido(int ganadorId)
    {
        var ganador = _batalla.Jugador1;
        var perdedor = _batalla.Jugador2;
        if(ganadorId == IdJugador2)
        {
            ganador = _batalla.Jugador2;
            perdedor = _batalla.Jugador1;
        }

        ganador.ActualizarElo(EloPrevioGanador);
        perdedor.ActualizarElo(EloPrevioPerdedor);
        _inscripcion = new Inscripcion(_batalla.Torneo, perdedor, FechaActual.ToDateTime(TimeOnly.MinValue)) { Id = 20 };
        _batallas.Setup(r => r.ObtenerPorId(BatallaId)).Returns(_batalla);
        _inscripciones.Setup(r => r.ObtenerAceptada(TorneoId, perdedor.Id)).Returns(_inscripcion);
        _elo.Setup(e => e.Calcular(EloPrevioGanador, EloPrevioPerdedor)).Returns(new ResultadoElo(EloNuevoGanador, EloNuevoPerdedor));
        _batallas.Setup(r => r.Guardar());
    }

    // Lleva la batalla al estado pedido con operaciones del dominio, sin reflexión. Pendiente es el estado inicial.
    private void LlevarBatallaA(EstadoBatalla estado)
    {
        if(estado == EstadoBatalla.EnArbitraje)
        {
            CompletarLosOchoLanzamientos();
        }

        if(estado == EstadoBatalla.Finalizada)
        {
            _batalla.Fallar(IdJugador2, "Fallo anterior de la batalla.", -16, 16);
        }

        if(estado == EstadoBatalla.Cancelada)
        {
            _batalla.Cancelar();
        }
    }

    // Los dos jugadores declaran las mismas cuatro skills y las lanzan alternando; empieza el jugador 1 (sorteo).
    private void CompletarLosOchoLanzamientos()
    {
        for(var id = 1; id <= 4; id++)
        {
            var skill = new Skill($"SKILL{id}", $"Skill {id}", "Skill para disputar una batalla de aura.", CategoriaSkill.Gestual, 2) { Id = id };
            _batalla.Jugador1.DeclararSkill(skill, 4, FechaActual);
            _batalla.Jugador2.DeclararSkill(skill, 4, FechaActual);
        }

        var fecha = new DateTimeOffset(2026, 10, 2, 16, 0, 0, TimeSpan.Zero);
        for(var id = 1; id <= 4; id++)
        {
            var skill = _batalla.Jugador1.Repertorio.Single(d => d.SkillId == id).Skill;
            _batalla.Lanzar(_batalla.Jugador1, skill, fecha);
            _batalla.Lanzar(_batalla.Jugador2, skill, fecha);
        }
    }

    private void AssertSinEfectos(EstadoBatalla estadoEsperado)
    {
        _elo.Verify(e => e.Calcular(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        _batallas.Verify(r => r.Guardar(), Times.Never);
        Assert.AreEqual(estadoEsperado, _batalla.Estado);
        Assert.AreEqual(EloPrevioGanador, _batalla.Jugador1.Elo);
        Assert.AreEqual(EloPrevioPerdedor, _batalla.Jugador2.Elo);
        Assert.AreEqual(EstadoInscripcion.Aceptada, _inscripcion.Estado);
    }

    private static FalloRequest CrearRequest(int ganadorId)
    {
        return new FalloRequest { GanadorId = ganadorId, Comentario = Comentario };
    }

    private static Usuario CrearUsuario(int id, Rol rol)
    {
        return new Usuario("Nombre", "Apellido", $"usuario{id}@farmeort.com", $"usuario{id}",
            new DateOnly(2000, 1, 1), rol, "hash", FechaActual)
        { Id = id };
    }
}
