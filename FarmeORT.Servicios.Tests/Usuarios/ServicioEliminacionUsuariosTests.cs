using FarmeORT.Dominio.Usuarios;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Batallas;
using FarmeORT.IRepositorios.Inscripciones;
using FarmeORT.IRepositorios.Torneos;
using FarmeORT.IRepositorios.Usuarios;
using FarmeORT.IServicios.Seguridad;
using FarmeORT.Servicios.Usuarios;
using Moq;

namespace FarmeORT.Servicios.Tests.Usuarios;

[TestClass]
public sealed class ServicioEliminacionUsuariosTests
{
    private const int AdministradorActualId = 1;
    private const int UsuarioId = 5;

    private Mock<IRepositorioUsuarios> _repositorioUsuarios = null!;
    private Mock<IRepositorioTorneos> _repositorioTorneos = null!;
    private Mock<IRepositorioBatallas> _repositorioBatallas = null!;
    private ServicioUsuarios _servicio = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _repositorioUsuarios = new Mock<IRepositorioUsuarios>(MockBehavior.Strict);
        _repositorioTorneos = new Mock<IRepositorioTorneos>(MockBehavior.Strict);
        _repositorioBatallas = new Mock<IRepositorioBatallas>(MockBehavior.Strict);
        var hasheadorContrasenas = new Mock<IHasheadorContrasenas>(MockBehavior.Strict);
        var reloj = new Mock<TimeProvider>(MockBehavior.Strict);

        // Por defecto el usuario no organiza torneos vigentes ni tiene batallas sin resolver; los tests de cada
        // regla lo redefinen.
        _repositorioTorneos.Setup(r => r.ExisteVigenteDelOrganizador(UsuarioId)).Returns(false);
        _repositorioBatallas.Setup(r => r.ExisteSinResolverDelJugador(UsuarioId)).Returns(false);
        _servicio = new ServicioUsuarios(
            _repositorioUsuarios.Object,
            _repositorioTorneos.Object,
            _repositorioBatallas.Object,
            new Mock<IRepositorioInscripciones>(MockBehavior.Strict).Object,
            hasheadorContrasenas.Object,
            reloj.Object);
    }

    [TestMethod]
    public void Eliminar_UsuarioActual_LanzaOperacionSobreSiMismoSinConsultarLaBase()
    {
        Assert.ThrowsExactly<OperacionSobreSiMismoException>(() => _servicio.Eliminar(AdministradorActualId, AdministradorActualId));
    }

    [TestMethod]
    public void Eliminar_UsuarioInexistente_LanzaUsuarioNoEncontrado()
    {
        _repositorioUsuarios.Setup(r => r.ObtenerPorId(99)).Returns((Usuario?)null);

        Assert.ThrowsExactly<UsuarioNoEncontradoException>(() => _servicio.Eliminar(99, AdministradorActualId));
    }

    [TestMethod]
    public void Eliminar_UsuarioExistente_LoDaDeBajaYGuardaLaBaja()
    {
        var usuario = CrearUsuario(Rol.Jugador);
        _repositorioUsuarios.Setup(r => r.ObtenerPorId(UsuarioId)).Returns(usuario);
        _repositorioUsuarios.Setup(r => r.Eliminar(usuario));

        _servicio.Eliminar(UsuarioId, AdministradorActualId);

        _repositorioUsuarios.Verify(r => r.Eliminar(It.Is<Usuario>(u => u == usuario && u.Eliminado)), Times.Once);
    }

    [TestMethod]
    public void Eliminar_OrganizadorDeTorneoVigente_LanzaUsuarioOrganizaTorneoVigenteSinDarDeBaja()
    {
        var organizador = CrearUsuario(Rol.Organizador);
        _repositorioUsuarios.Setup(r => r.ObtenerPorId(UsuarioId)).Returns(organizador);
        _repositorioTorneos.Setup(r => r.ExisteVigenteDelOrganizador(UsuarioId)).Returns(true);
        _repositorioUsuarios.Setup(r => r.Eliminar(organizador));

        Assert.ThrowsExactly<UsuarioOrganizaTorneoVigenteException>(() => _servicio.Eliminar(UsuarioId, AdministradorActualId));

        Assert.IsFalse(organizador.Eliminado);
        _repositorioUsuarios.Verify(r => r.Eliminar(It.IsAny<Usuario>()), Times.Never);
    }

    [TestMethod]
    public void Eliminar_JugadorConBatallasSinResolver_LanzaUsuarioConBatallasSinResolverSinDarDeBaja()
    {
        var jugador = CrearUsuario(Rol.Jugador);
        _repositorioUsuarios.Setup(r => r.ObtenerPorId(UsuarioId)).Returns(jugador);
        _repositorioBatallas.Setup(r => r.ExisteSinResolverDelJugador(UsuarioId)).Returns(true);
        _repositorioUsuarios.Setup(r => r.Eliminar(jugador));

        Assert.ThrowsExactly<UsuarioConBatallasSinResolverException>(() => _servicio.Eliminar(UsuarioId, AdministradorActualId));

        Assert.IsFalse(jugador.Eliminado);
        _repositorioUsuarios.Verify(r => r.Eliminar(It.IsAny<Usuario>()), Times.Never);
    }

    private static Usuario CrearUsuario(Rol rol)
    {
        return new Usuario("Carlos", "Suarez", "carlos@farmeort.com", "carlossuarez", new DateOnly(2001, 8, 2), rol, "hash", new DateOnly(2026, 9, 30))
        {
            Id = UsuarioId,
        };
    }
}
