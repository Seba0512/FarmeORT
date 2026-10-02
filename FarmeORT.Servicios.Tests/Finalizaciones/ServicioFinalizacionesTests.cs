using FarmeORT.Dominio.Inscripciones;
using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.DTOs.Usuarios;
using FarmeORT.IRepositorios.Batallas;
using FarmeORT.IRepositorios.Inscripciones;
using FarmeORT.IRepositorios.Torneos;
using FarmeORT.Servicios.Finalizaciones;
using Moq;

namespace FarmeORT.Servicios.Tests.Finalizaciones;

[TestClass]
public sealed class ServicioFinalizacionesTests
{
    private const int TorneoId = 4;
    private const int OrganizadorId = 2;

    private static readonly DateOnly FechaActual = new(2026, 10, 2);

    private Mock<IRepositorioTorneos> _repositorioTorneos = null!;
    private Mock<IRepositorioInscripciones> _repositorioInscripciones = null!;
    private Mock<IRepositorioBatallas> _repositorioBatallas = null!;
    private Torneo _torneo = null!;
    private Usuario _campeon = null!;
    private ServicioFinalizaciones _servicio = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _repositorioTorneos = new Mock<IRepositorioTorneos>(MockBehavior.Strict);
        _repositorioInscripciones = new Mock<IRepositorioInscripciones>(MockBehavior.Strict);
        _repositorioBatallas = new Mock<IRepositorioBatallas>(MockBehavior.Strict);
        _torneo = new Torneo("COPAAURA", "Copa Aura", "Torneo eliminatorio de batallas de aura.", FechaActual, FechaActual.AddDays(10), 4, CrearUsuario(OrganizadorId, Rol.Organizador), FechaActual)
        {
            Id = TorneoId,
        };
        _campeon = CrearUsuario(7, Rol.Jugador);
        _servicio = new ServicioFinalizaciones(_repositorioTorneos.Object, _repositorioInscripciones.Object, _repositorioBatallas.Object);
    }

    [TestMethod]
    public void Finalizar_TorneoConUnSoloJugadorSinEliminar_DevuelveAlCampeon()
    {
        PrepararTorneoListoParaFinalizar();

        var finalizacion = _servicio.Finalizar(TorneoId, OrganizadorId, Roles.Organizador);

        Assert.AreEqual(TorneoId, finalizacion.TorneoId);
        Assert.AreEqual("Finalizado", finalizacion.Estado);
        Assert.AreEqual(7, finalizacion.Campeon.Id);
        Assert.AreEqual("usuario7", finalizacion.Campeon.Nickname);
    }

    [TestMethod]
    public void Finalizar_TorneoConUnSoloJugadorSinEliminar_LoDejaFinalizadoConSuCampeonYGuardaUnaVez()
    {
        PrepararTorneoListoParaFinalizar();

        _servicio.Finalizar(TorneoId, OrganizadorId, Roles.Organizador);

        Assert.AreEqual(EstadoTorneo.Finalizado, _torneo.Estado);
        Assert.AreEqual(7, _torneo.CampeonId);
        Assert.AreSame(_campeon, _torneo.Campeon);
        _repositorioTorneos.Verify(r => r.Actualizar(_torneo), Times.Once);
    }

    // Torneo En curso, sin batallas sin resolver y con un solo jugador con inscripción Aceptada: el campeón.
    private void PrepararTorneoListoParaFinalizar()
    {
        var campeon = new Inscripcion(_torneo, _campeon, new DateTime(2026, 10, 2));
        _torneo.Iniciar(4);
        _repositorioTorneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns(_torneo);
        _repositorioBatallas.Setup(r => r.ListarSinResolverDelTorneo(TorneoId)).Returns([]);
        _repositorioInscripciones.Setup(r => r.ListarAceptadasDelTorneo(TorneoId)).Returns([campeon]);
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
