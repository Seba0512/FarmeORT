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
