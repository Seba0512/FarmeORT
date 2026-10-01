using FarmeORT.Dominio.Usuarios;
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
    private const int UsuarioId = 5;
    private const string Contrasena = "ClaveNueva123!!";
    private const string ContrasenaHash = "hash-nuevo";

    private static readonly DateOnly FechaActual = new(2026, 9, 30);

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

    [TestMethod]
    public void Modificar_UsuarioInexistente_LanzaUsuarioNoEncontrado()
    {
        _repositorioUsuarios.Setup(r => r.ObtenerPorId(99)).Returns((Usuario?)null);

        Assert.ThrowsExactly<UsuarioNoEncontradoException>(
            () => _servicio.Modificar(99, CrearRequest(), AdministradorActualId));
    }

    [TestMethod]
    public void Modificar_DatosValidos_ActualizaElUsuarioConContrasenaHasheada()
    {
        var usuario = ConfigurarModificacionValida();

        _servicio.Modificar(UsuarioId, CrearRequest(), AdministradorActualId);

        _repositorioUsuarios.Verify(
            r => r.Actualizar(It.Is<Usuario>(u =>
                u == usuario
                && u.Nombre == "Ana María"
                && u.Apellido == "Gómez"
                && u.Email == "anamaria@farmeort.com"
                && u.Nickname == "anamaria"
                && u.FechaNacimiento == new DateOnly(1999, 4, 20)
                && u.Rol == Rol.Jugador
                && u.ContrasenaHash == ContrasenaHash)),
            Times.Once);
    }

    [TestMethod]
    public void Modificar_DatosValidos_DevuelveElUsuarioModificado()
    {
        ConfigurarModificacionValida();

        var response = _servicio.Modificar(UsuarioId, CrearRequest(), AdministradorActualId);

        Assert.AreEqual(UsuarioId, response.Id);
        Assert.AreEqual("Ana María", response.Nombre);
        Assert.AreEqual("Gómez", response.Apellido);
        Assert.AreEqual("anamaria@farmeort.com", response.Email);
        Assert.AreEqual("anamaria", response.Nickname);
        Assert.AreEqual(new DateOnly(1999, 4, 20), response.FechaNacimiento);
        Assert.AreEqual(Roles.Jugador, response.Rol);
        Assert.AreEqual(1200, response.Elo);
    }

    private Usuario ConfigurarModificacionValida()
    {
        var usuario = CrearUsuarioExistente();
        _repositorioUsuarios.Setup(r => r.ObtenerPorId(UsuarioId)).Returns(usuario);
        _repositorioUsuarios.Setup(r => r.ObtenerPorEmail("anamaria@farmeort.com")).Returns((Usuario?)null);
        _repositorioUsuarios.Setup(r => r.ExistePorNickname("anamaria")).Returns(false);
        _hasheadorContrasenas.Setup(h => h.Hashear(Contrasena)).Returns(ContrasenaHash);
        _reloj.Setup(r => r.GetUtcNow()).Returns(new DateTimeOffset(2026, 9, 30, 12, 0, 0, TimeSpan.Zero));
        _reloj.Setup(r => r.LocalTimeZone).Returns(TimeZoneInfo.Utc);
        _repositorioUsuarios.Setup(r => r.Actualizar(usuario));
        return usuario;
    }

    private static Usuario CrearUsuarioExistente()
    {
        return new Usuario("Ana", "Pérez", "ana@farmeort.com", "anaperez", new DateOnly(2000, 1, 15), Rol.Organizador, "hash-anterior", FechaActual)
        {
            Id = UsuarioId,
        };
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
