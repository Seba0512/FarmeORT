using FarmeORT.DTOs.Usuarios;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Usuarios;
using FarmeORT.IServicios.Seguridad;
using FarmeORT.Servicios.Usuarios;
using Moq;

namespace FarmeORT.Servicios.Tests.Usuarios;

[TestClass]
public sealed class ServicioModificacionUsuariosTests
{
    private const int AdministradorActualId = 1;
    private const string Contrasena = "ClaveNueva123!!";

    private Mock<IRepositorioUsuarios> _repositorioUsuarios = null!;
    private Mock<IHasheadorContrasenas> _hasheadorContrasenas = null!;
    private Mock<TimeProvider> _reloj = null!;
    private ServicioUsuarios _servicio = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _repositorioUsuarios = new Mock<IRepositorioUsuarios>(MockBehavior.Strict);
        _hasheadorContrasenas = new Mock<IHasheadorContrasenas>(MockBehavior.Strict);
        _reloj = new Mock<TimeProvider>(MockBehavior.Strict);
        _servicio = new ServicioUsuarios(_repositorioUsuarios.Object, _hasheadorContrasenas.Object, _reloj.Object);
    }

    [TestMethod]
    public void Modificar_UsuarioActual_LanzaOperacionSobreSiMismoSinConsultarLaBase()
    {
        Assert.ThrowsExactly<OperacionSobreSiMismoException>(
            () => _servicio.Modificar(AdministradorActualId, CrearRequest(), AdministradorActualId));
    }

    private static UsuarioAdminRequest CrearRequest()
    {
        return new UsuarioAdminRequest
        {
            Nombre = "Ana María",
            Apellido = "Gómez",
            Email = "anamaria@farmeort.com",
            Nickname = "anamaria",
            Contrasena = Contrasena,
            FechaNacimiento = new DateOnly(1999, 4, 20),
            Rol = Roles.Jugador,
        };
    }
}
