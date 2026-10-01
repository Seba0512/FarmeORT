using FarmeORT.Dominio.Usuarios;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Usuarios;
using FarmeORT.IServicios.Seguridad;
using FarmeORT.Servicios.Usuarios;
using Moq;

namespace FarmeORT.Servicios.Tests.Usuarios;

[TestClass]
public sealed class ServicioEliminacionUsuariosTests
{
    private const int AdministradorActualId = 1;

    private Mock<IRepositorioUsuarios> _repositorioUsuarios = null!;
    private ServicioUsuarios _servicio = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _repositorioUsuarios = new Mock<IRepositorioUsuarios>(MockBehavior.Strict);
        var hasheadorContrasenas = new Mock<IHasheadorContrasenas>(MockBehavior.Strict);
        var reloj = new Mock<TimeProvider>(MockBehavior.Strict);
        _servicio = new ServicioUsuarios(_repositorioUsuarios.Object, hasheadorContrasenas.Object, reloj.Object);
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
        var usuario = new Usuario("Carlos", "Suarez", "carlos@farmeort.com", "carlossuarez", new DateOnly(2001, 8, 2), Rol.Jugador, "hash", new DateOnly(2026, 9, 30))
        {
            Id = 5,
        };
        _repositorioUsuarios.Setup(r => r.ObtenerPorId(5)).Returns(usuario);
        _repositorioUsuarios.Setup(r => r.Eliminar(usuario));

        _servicio.Eliminar(5, AdministradorActualId);

        _repositorioUsuarios.Verify(r => r.Eliminar(It.Is<Usuario>(u => u == usuario && u.Eliminado)), Times.Once);
    }
}
