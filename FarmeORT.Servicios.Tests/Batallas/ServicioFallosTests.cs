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
        var jugador1 = CrearUsuario(7, Rol.Jugador);
        var jugador2 = CrearUsuario(8, Rol.Jugador);
        var torneo = new Torneo("COPAAURA", "Copa Aura", "Torneo eliminatorio de batallas de aura.",
            FechaActual.AddDays(1), FechaActual.AddDays(2), 4, CrearUsuario(2, Rol.Organizador), FechaActual) { Id = 5 };
        var sorteo = new Mock<ISorteo>(MockBehavior.Strict);
        sorteo.Setup(s => s.ElegirEntre(jugador1, jugador2)).Returns(jugador1);
        _batalla = new Batalla(torneo, 1, jugador1, jugador2, sorteo.Object) { Id = 12 };
        _inscripcion = new Inscripcion(torneo, jugador2, FechaActual.ToDateTime(TimeOnly.MinValue)) { Id = 20 };
    }

    [TestMethod]
    public void Fallar_BatallaConLanzamientos_DevuelveHistorialOrdenado()
    {
        ConfigurarFalloValido();
        var skill = new Skill("SIXSEVEN", "Six-Seven", "Gesto alternando las manos hacia arriba y abajo.", CategoriaSkill.Gestual, 2) { Id = 3 };
        _batalla.Jugador1.DeclararSkill(skill, 4, FechaActual);
        _batalla.Jugador2.DeclararSkill(skill, 4, FechaActual);
        var fecha = new DateTimeOffset(2026, 10, 2, 16, 30, 0, TimeSpan.Zero);
        _batalla.Lanzar(_batalla.Jugador1, skill, fecha);
        _batalla.Lanzar(_batalla.Jugador2, skill, fecha.AddMinutes(1));

        var resultado = _servicio.Fallar(12, 2, RequestValido());

        CollectionAssert.AreEqual(new[] { 1, 2 }, resultado.Lanzamientos.Select(l => l.Turno).ToArray());
        Assert.AreEqual(7, resultado.Lanzamientos[0].JugadorId);
        Assert.AreEqual("usuario7", resultado.Lanzamientos[0].Nickname);
        Assert.AreEqual(3, resultado.Lanzamientos[0].SkillId);
        Assert.AreEqual("Six-Seven", resultado.Lanzamientos[0].Skill);
        Assert.AreEqual(fecha, resultado.Lanzamientos[0].Fecha);
    }

    [TestMethod]
    public void Fallar_PerdedorSinInscripcionAceptada_RechazaSinEfectos()
    {
        ConfigurarFalloValido();
        _inscripciones.Setup(r => r.ObtenerAceptada(5, 8)).Returns((Inscripcion?)null);

        Assert.ThrowsExactly<InscripcionNoActivaException>(() => _servicio.Fallar(12, 2, RequestValido()));

        _elo.Verify(e => e.Calcular(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        _batallas.Verify(r => r.Guardar(), Times.Never);
        Assert.AreEqual(1200, _batalla.Jugador1.Elo);
        Assert.AreEqual(1200, _batalla.Jugador2.Elo);
        Assert.AreEqual(EstadoBatalla.Pendiente, _batalla.Estado);
    }

    [TestMethod]
    [DataRow(-1)]
    [DataRow(-2)]
    [DataRow(0)]
    [DataRow(9)]
    [DataRow(501)]
    public void Fallar_ComentarioInvalido_RechazaSinEfectos(int largo)
    {
        ConfigurarFalloValido();
        var comentario = largo == -1 ? null : largo == -2 ? new string(' ', 20) : new string('a', largo);
        var request = new FalloRequest { GanadorId = 7, Comentario = comentario };

        Assert.ThrowsExactly<DatosInvalidosException>(() => _servicio.Fallar(12, 2, request));

        _elo.Verify(e => e.Calcular(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        _batallas.Verify(r => r.Guardar(), Times.Never);
        Assert.AreEqual(1200, _batalla.Jugador1.Elo);
        Assert.AreEqual(EstadoBatalla.Pendiente, _batalla.Estado);
        Assert.AreEqual(EstadoInscripcion.Aceptada, _inscripcion.Estado);
    }

    [TestMethod]
    public void Fallar_GanadorAjeno_RechazaSinEfectos()
    {
        ConfigurarFalloValido();
        _inscripciones.Setup(r => r.ObtenerAceptada(5, 7)).Returns(_inscripcion);
        var request = new FalloRequest { GanadorId = 9, Comentario = "Victoria por abandono." };

        Assert.ThrowsExactly<JugadorNoParticipaEnBatallaException>(() => _servicio.Fallar(12, 2, request));

        _elo.Verify(e => e.Calcular(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        _batallas.Verify(r => r.Guardar(), Times.Never);
        Assert.AreEqual(EstadoBatalla.Pendiente, _batalla.Estado);
        Assert.AreEqual(1200, _batalla.Jugador1.Elo);
        Assert.AreEqual(EstadoInscripcion.Aceptada, _inscripcion.Estado);
    }

    [TestMethod]
    [DataRow(EstadoBatalla.Finalizada)]
    [DataRow(EstadoBatalla.Cancelada)]
    public void Fallar_BatallaResuelta_RechazaSinEfectos(EstadoBatalla estado)
    {
        ConfigurarFalloValido();
        typeof(Batalla).GetProperty(nameof(Batalla.Estado))!.SetValue(_batalla, estado);

        Assert.ThrowsExactly<BatallaNoFallableException>(() => _servicio.Fallar(12, 2, RequestValido()));

        _elo.Verify(e => e.Calcular(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        _batallas.Verify(r => r.Guardar(), Times.Never);
        Assert.AreEqual(estado, _batalla.Estado);
        Assert.AreEqual(1200, _batalla.Jugador1.Elo);
        Assert.AreEqual(EstadoInscripcion.Aceptada, _inscripcion.Estado);
    }

    [TestMethod]
    [DataRow(7, EstadoBatalla.Pendiente)]
    [DataRow(8, EstadoBatalla.Pendiente)]
    [DataRow(7, EstadoBatalla.EnArbitraje)]
    [DataRow(8, EstadoBatalla.EnArbitraje)]
    public void Fallar_DatosValidos_AplicaEfectosEnOrdenConEloPrevioYUnGuardado(int ganadorId, EstadoBatalla estado)
    {
        typeof(Batalla).GetProperty(nameof(Batalla.Estado))!.SetValue(_batalla, estado);
        var ganador = ganadorId == 7 ? _batalla.Jugador1 : _batalla.Jugador2;
        var perdedor = ganadorId == 7 ? _batalla.Jugador2 : _batalla.Jugador1;
        typeof(Usuario).GetProperty(nameof(Usuario.Elo))!.SetValue(ganador, 1500);
        typeof(Usuario).GetProperty(nameof(Usuario.Elo))!.SetValue(perdedor, 1100);
        _inscripcion = new Inscripcion(_batalla.Torneo, perdedor, FechaActual.ToDateTime(TimeOnly.MinValue));
        _batallas.Setup(r => r.ObtenerPorId(12)).Returns(_batalla);
        _inscripciones.Setup(r => r.ObtenerAceptada(5, perdedor.Id)).Returns(_inscripcion);
        _elo.Setup(e => e.Calcular(1500, 1100)).Callback(() =>
        {
            Assert.AreEqual(1500, ganador.Elo);
            Assert.AreEqual(1100, perdedor.Elo);
            Assert.AreEqual(estado, _batalla.Estado);
            Assert.AreEqual(EstadoInscripcion.Aceptada, _inscripcion.Estado);
        }).Returns(new ResultadoElo(1530, 1080));
        _batallas.Setup(r => r.Guardar()).Callback(() =>
        {
            Assert.AreEqual(1530, ganador.Elo);
            Assert.AreEqual(1080, perdedor.Elo);
            Assert.AreEqual(EstadoBatalla.Finalizada, _batalla.Estado);
            Assert.AreEqual(ganadorId, _batalla.GanadorId);
            Assert.AreSame(ganador, _batalla.Ganador);
            Assert.AreEqual("Victoria por abandono.", _batalla.Comentario);
            Assert.IsNull(_batalla.JugadorTurnoId);
            Assert.AreEqual(EstadoInscripcion.Eliminada, _inscripcion.Estado);
        });
        var request = new FalloRequest { GanadorId = ganadorId, Comentario = "Victoria por abandono." };

        var resultado = _servicio.Fallar(12, 2, request);

        _elo.Verify(e => e.Calcular(1500, 1100), Times.Once);
        _batallas.Verify(r => r.Guardar(), Times.Once);
        _inscripciones.Verify(r => r.Actualizar(It.IsAny<Inscripcion>()), Times.Never);
        Assert.AreEqual(12, resultado.Id);
        Assert.AreEqual(5, resultado.TorneoId);
        Assert.AreEqual(1, resultado.Ronda);
        Assert.AreEqual("Finalizada", resultado.Estado);
        Assert.AreEqual(ganadorId, resultado.GanadorId);
        Assert.AreEqual("Victoria por abandono.", resultado.Comentario);
        Assert.AreEqual(7, resultado.Jugador1.Id);
        Assert.AreEqual(8, resultado.Jugador2.Id);
        Assert.AreEqual("usuario7", resultado.Jugador1.Nickname);
        Assert.AreEqual("usuario8", resultado.Jugador2.Nickname);
        Assert.AreEqual(ganadorId == 7 ? 1530 : 1080, resultado.Jugador1.Elo);
        Assert.AreEqual(ganadorId == 8 ? 1530 : 1080, resultado.Jugador2.Elo);
        Assert.AreEqual(ganadorId == 7 ? 30 : -20, resultado.CambioEloJugador1);
        Assert.AreEqual(ganadorId == 8 ? 30 : -20, resultado.CambioEloJugador2);
        Assert.IsNull(resultado.JugadorTurnoId);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow(0)]
    [DataRow(-1)]
    public void Fallar_GanadorAusenteONoPositivo_RechazaSinConsultar(int? ganadorId)
    {
        _batallas.Setup(r => r.ObtenerPorId(12)).Returns(_batalla);
        var request = new FalloRequest { GanadorId = ganadorId, Comentario = "Victoria por abandono." };

        Assert.ThrowsExactly<DatosInvalidosException>(() => _servicio.Fallar(12, 2, request));

        _batallas.Verify(r => r.ObtenerPorId(It.IsAny<int>()), Times.Never);
    }

    [TestMethod]
    public void Fallar_OrganizadorAjeno_RechazaSinEfectos()
    {
        _batallas.Setup(r => r.ObtenerPorId(12)).Returns(_batalla);

        Assert.ThrowsExactly<NoEsOrganizadorDelTorneoException>(() => _servicio.Fallar(12, 3, RequestValido()));

        _batallas.Verify(r => r.Guardar(), Times.Never);
        _elo.Verify(e => e.Calcular(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        Assert.AreEqual(EstadoBatalla.Pendiente, _batalla.Estado);
        Assert.AreEqual(EstadoInscripcion.Aceptada, _inscripcion.Estado);
    }

    [TestMethod]
    public void Fallar_BatallaInexistente_RechazaSinCalcularNiGuardar()
    {
        _batallas.Setup(r => r.ObtenerPorId(99)).Returns((Batalla?)null);

        Assert.ThrowsExactly<BatallaNoEncontradaException>(() => _servicio.Fallar(99, 2, RequestValido()));

        _elo.Verify(e => e.Calcular(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        _batallas.Verify(r => r.Guardar(), Times.Never);
    }

    private void ConfigurarFalloValido()
    {
        _batallas.Setup(r => r.ObtenerPorId(12)).Returns(_batalla);
        _inscripciones.Setup(r => r.ObtenerAceptada(5, 8)).Returns(_inscripcion);
        _elo.Setup(e => e.Calcular(1200, 1200)).Returns(new ResultadoElo(1216, 1184));
        _batallas.Setup(r => r.Guardar());
    }

    private static FalloRequest RequestValido()
    {
        return new FalloRequest { GanadorId = 7, Comentario = "Victoria por abandono." };
    }

    private static Usuario CrearUsuario(int id, Rol rol)
    {
        return new Usuario("Nombre", "Apellido", $"usuario{id}@farmeort.com", $"usuario{id}",
            new DateOnly(2000, 1, 1), rol, "hash", FechaActual) { Id = id };
    }
}
