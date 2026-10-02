using FarmeORT.Dominio.Batallas;
using FarmeORT.Dominio.Elo;
using FarmeORT.Dominio.Inscripciones;
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
