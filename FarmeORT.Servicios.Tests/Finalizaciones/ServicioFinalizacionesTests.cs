using FarmeORT.Dominio.Batallas;
using FarmeORT.Dominio.Inscripciones;
using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.DTOs.Usuarios;
using FarmeORT.Excepciones;
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

    [TestMethod]
    public void Finalizar_TorneoInexistente_LanzaTorneoNoEncontrado()
    {
        _repositorioTorneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns((Torneo?)null);

        Assert.ThrowsExactly<TorneoNoEncontradoException>(() => _servicio.Finalizar(TorneoId, OrganizadorId, Roles.Organizador));
    }

    [TestMethod]
    public void Finalizar_OrganizadorNoAsignado_LanzaNoEsOrganizadorDelTorneo()
    {
        _repositorioTorneos.Setup(r => r.ObtenerPorId(TorneoId)).Returns(_torneo);

        Assert.ThrowsExactly<NoEsOrganizadorDelTorneoException>(() => _servicio.Finalizar(TorneoId, 5, Roles.Organizador));
        _repositorioTorneos.Verify(r => r.Actualizar(It.IsAny<Torneo>()), Times.Never);
    }

    [TestMethod]
    public void Finalizar_AdministradorQueNoEsElOrganizador_FinalizaElTorneo()
    {
        PrepararTorneoListoParaFinalizar();

        var finalizacion = _servicio.Finalizar(TorneoId, 1, Roles.Administrador);

        Assert.AreEqual("Finalizado", finalizacion.Estado);
    }

    [TestMethod]
    public void Finalizar_TorneoConBatallasSinResolver_LanzaTorneoConBatallasSinResolver()
    {
        PrepararTorneoListoParaFinalizar();
        var rival = CrearUsuario(8, Rol.Jugador);
        var sorteo = new Mock<ISorteo>(MockBehavior.Strict);
        sorteo.Setup(s => s.ElegirEntre(_campeon, rival)).Returns(_campeon);
        _repositorioBatallas.Setup(r => r.ListarSinResolverDelTorneo(TorneoId)).Returns([new Batalla(_torneo, 2, _campeon, rival, sorteo.Object)]);

        Assert.ThrowsExactly<TorneoConBatallasSinResolverException>(() => _servicio.Finalizar(TorneoId, OrganizadorId, Roles.Organizador));
        Assert.AreEqual(EstadoTorneo.EnCurso, _torneo.Estado);
        _repositorioTorneos.Verify(r => r.Actualizar(It.IsAny<Torneo>()), Times.Never);
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
