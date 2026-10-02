using FarmeORT.Dominio.Usuarios;
using FarmeORT.DTOs.Usuarios;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Torneos;
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
    private Mock<IRepositorioTorneos> _repositorioTorneos = null!;
    private Mock<IHasheadorContrasenas> _hasheadorContrasenas = null!;
    private Mock<TimeProvider> _reloj = null!;
    private ServicioUsuarios _servicio = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _repositorioUsuarios = new Mock<IRepositorioUsuarios>(MockBehavior.Strict);
        _repositorioTorneos = new Mock<IRepositorioTorneos>(MockBehavior.Strict);
        _hasheadorContrasenas = new Mock<IHasheadorContrasenas>(MockBehavior.Strict);
        _reloj = new Mock<TimeProvider>(MockBehavior.Strict);
        _servicio = new ServicioUsuarios(
            _repositorioUsuarios.Object,
            _repositorioTorneos.Object,
            _hasheadorContrasenas.Object,
            _reloj.Object);
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

    [TestMethod]
    public void Modificar_EmailDeOtroUsuario_LanzaEmailYaExisteSinGuardar()
    {
        ConfigurarModificacionValida();
        var otro = new Usuario("Otra", "Persona", "anamaria@farmeort.com", "otrapersona", new DateOnly(1995, 5, 20), Rol.Jugador, "hash", FechaActual)
        {
            Id = 8,
        };
        _repositorioUsuarios.Setup(r => r.ObtenerPorEmail("anamaria@farmeort.com")).Returns(otro);

        Assert.ThrowsExactly<EmailYaExisteException>(() => _servicio.Modificar(UsuarioId, CrearRequest(), AdministradorActualId));

        _repositorioUsuarios.Verify(r => r.Actualizar(It.IsAny<Usuario>()), Times.Never);
    }

    [TestMethod]
    public void Modificar_NicknameDeOtroUsuario_LanzaNicknameYaExisteSinGuardar()
    {
        ConfigurarModificacionValida();
        _repositorioUsuarios.Setup(r => r.ExistePorNickname("anamaria")).Returns(true);

        Assert.ThrowsExactly<NicknameYaExisteException>(() => _servicio.Modificar(UsuarioId, CrearRequest(), AdministradorActualId));

        _repositorioUsuarios.Verify(r => r.Actualizar(It.IsAny<Usuario>()), Times.Never);
    }

    [TestMethod]
    [DataRow("ana@farmeort.com", "anaperez")]
    [DataRow("ANA@farmeort.com", "AnaPerez")]
    public void Modificar_ConservaSuEmailYNickname_NoLoConsideraDuplicado(string email, string nickname)
    {
        var usuario = ConfigurarModificacionValida();
        _repositorioUsuarios.Setup(r => r.ObtenerPorEmail(email)).Returns(usuario);
        _repositorioUsuarios.Setup(r => r.ExistePorNickname(nickname)).Returns(true);
        var request = new UsuarioAdminRequest
        {
            Nombre = "Ana María",
            Apellido = "Gómez",
            Email = email,
            Nickname = nickname,
            Contrasena = Contrasena,
            FechaNacimiento = new DateOnly(1999, 4, 20),
            Rol = Roles.Jugador,
        };

        var response = _servicio.Modificar(UsuarioId, request, AdministradorActualId);

        Assert.AreEqual(email, response.Email);
        Assert.AreEqual(nickname, response.Nickname);
        _repositorioUsuarios.Verify(r => r.Actualizar(usuario), Times.Once);
    }

    [TestMethod]
    public void Modificar_ContrasenaInvalida_LanzaDatosInvalidosSinConsultarLaBase()
    {
        var request = CrearRequest(contrasena: "corta");

        Assert.ThrowsExactly<DatosInvalidosException>(() => _servicio.Modificar(UsuarioId, request, AdministradorActualId));
    }

    [TestMethod]
    public void Modificar_RolInexistente_LanzaDatosInvalidosSinConsultarLaBase()
    {
        var request = CrearRequest(rol: "Superusuario");

        Assert.ThrowsExactly<DatosInvalidosException>(() => _servicio.Modificar(UsuarioId, request, AdministradorActualId));
    }

    [TestMethod]
    public void Modificar_FechaNacimientoOmitida_LanzaDatosInvalidosSinConsultarLaBase()
    {
        var request = new UsuarioAdminRequest
        {
            Nombre = "Ana María",
            Apellido = "Gómez",
            Email = "anamaria@farmeort.com",
            Nickname = "anamaria",
            Contrasena = Contrasena,
            Rol = Roles.Jugador,
        };

        var excepcion = Assert.ThrowsExactly<DatosInvalidosException>(() => _servicio.Modificar(UsuarioId, request, AdministradorActualId));

        Assert.AreEqual("La fecha de nacimiento es obligatoria.", excepcion.Message);
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

    private static UsuarioAdminRequest CrearRequest(string contrasena = Contrasena, string rol = Roles.Jugador)
    {
        return new UsuarioAdminRequest
        {
            Nombre = "Ana María",
            Apellido = "Gómez",
            Email = "anamaria@farmeort.com",
            Nickname = "anamaria",
            Contrasena = contrasena,
            FechaNacimiento = new DateOnly(1999, 4, 20),
            Rol = rol,
        };
    }
}
