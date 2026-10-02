using FarmeORT.Dominio.Usuarios;
using FarmeORT.DTOs.Usuarios;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Batallas;
using FarmeORT.IRepositorios.Torneos;
using FarmeORT.IRepositorios.Usuarios;
using FarmeORT.IServicios.Seguridad;
using FarmeORT.Servicios.Usuarios;
using Moq;

namespace FarmeORT.Servicios.Tests.Usuarios;

[TestClass]
public sealed class ServicioUsuariosTests
{
    private const string Email = "ana@farmeort.com";
    private const string Nickname = "anaperez";
    private const string Contrasena = "ClaveSegura123!!";
    private const string ContrasenaHash = "hash-generado";

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
        _servicio = new ServicioUsuarios(
            _repositorioUsuarios.Object,
            new Mock<IRepositorioTorneos>(MockBehavior.Strict).Object,
            new Mock<IRepositorioBatallas>(MockBehavior.Strict).Object,
            _hasheadorContrasenas.Object, _reloj.Object);
    }

    [TestMethod]
    public void Crear_EmailExistente_LanzaEmailYaExiste()
    {
        var existente = new Usuario("Otra", "Persona", Email, "otrapersona", new DateOnly(1995, 5, 20), Rol.Jugador, "hash", FechaActual);
        _repositorioUsuarios.Setup(r => r.ObtenerPorEmail(Email)).Returns(existente);

        Assert.ThrowsExactly<EmailYaExisteException>(() => _servicio.Crear(CrearRequest()));
    }

    [TestMethod]
    public void Crear_NicknameExistente_LanzaNicknameYaExiste()
    {
        _repositorioUsuarios.Setup(r => r.ObtenerPorEmail(Email)).Returns((Usuario?)null);
        _repositorioUsuarios.Setup(r => r.ExistePorNickname(Nickname)).Returns(true);

        Assert.ThrowsExactly<NicknameYaExisteException>(() => _servicio.Crear(CrearRequest()));
    }

    [TestMethod]
    public void Crear_DatosValidos_GuardaUsuarioConContrasenaHasheada()
    {
        ConfigurarAltaValida();

        _servicio.Crear(CrearRequest());

        _repositorioUsuarios.Verify(
            r => r.Agregar(It.Is<Usuario>(u =>
                u.Nombre == "Ana"
                && u.Apellido == "Pérez"
                && u.Email == Email
                && u.Nickname == Nickname
                && u.FechaNacimiento == new DateOnly(2000, 1, 15)
                && u.Rol == Rol.Organizador
                && u.ContrasenaHash == ContrasenaHash)),
            Times.Once);
    }

    [TestMethod]
    [DataRow(Roles.Organizador, null)]
    [DataRow(Roles.Jugador, 1200)]
    public void Crear_DatosValidos_DevuelveElUsuarioCreado(string rol, int? eloEsperado)
    {
        ConfigurarAltaValida();

        var response = _servicio.Crear(CrearRequest(rol));

        Assert.AreEqual("Ana", response.Nombre);
        Assert.AreEqual("Pérez", response.Apellido);
        Assert.AreEqual(Email, response.Email);
        Assert.AreEqual(Nickname, response.Nickname);
        Assert.AreEqual(new DateOnly(2000, 1, 15), response.FechaNacimiento);
        Assert.AreEqual(rol, response.Rol);
        Assert.AreEqual(eloEsperado, response.Elo);
    }

    [TestMethod]
    public void Crear_ContrasenaInvalida_LanzaDatosInvalidosSinConsultarLaBase()
    {
        var request = CrearRequest(contrasena: "corta");

        Assert.ThrowsExactly<DatosInvalidosException>(() => _servicio.Crear(request));
    }

    [TestMethod]
    public void Crear_FechaNacimientoOmitida_LanzaDatosInvalidosSinGuardarNiHashear()
    {
        ConfigurarAltaValida();
        var request = new UsuarioAdminRequest
        {
            Nombre = "Ana",
            Apellido = "Pérez",
            Email = Email,
            Nickname = Nickname,
            Contrasena = Contrasena,
            Rol = Roles.Organizador,
        };

        var excepcion = Assert.ThrowsExactly<DatosInvalidosException>(() => _servicio.Crear(request));

        Assert.AreEqual("La fecha de nacimiento es obligatoria.", excepcion.Message);
        _repositorioUsuarios.Verify(r => r.Agregar(It.IsAny<Usuario>()), Times.Never);
        _hasheadorContrasenas.Verify(h => h.Hashear(Contrasena), Times.Never);
    }

    [TestMethod]
    [DataRow("Superusuario")]
    [DataRow("5")]
    public void Crear_RolInexistente_LanzaDatosInvalidosSinConsultarLaBase(string rol)
    {
        var request = CrearRequest(rol);

        Assert.ThrowsExactly<DatosInvalidosException>(() => _servicio.Crear(request));
    }

    [TestMethod]
    public void ObtenerPorId_UsuarioInexistente_LanzaUsuarioNoEncontrado()
    {
        _repositorioUsuarios.Setup(r => r.ObtenerPorId(99)).Returns((Usuario?)null);

        Assert.ThrowsExactly<UsuarioNoEncontradoException>(() => _servicio.ObtenerPorId(99));
    }

    [TestMethod]
    public void ObtenerPorId_UsuarioExistente_DevuelveSusDatos()
    {
        var jugador = new Usuario("Carlos", "Suarez", "carlos@farmeort.com", "carlossuarez", new DateOnly(2001, 8, 2), Rol.Jugador, "hash", FechaActual)
        {
            Id = 3,
        };
        _repositorioUsuarios.Setup(r => r.ObtenerPorId(3)).Returns(jugador);

        var response = _servicio.ObtenerPorId(3);

        Assert.AreEqual(3, response.Id);
        Assert.AreEqual("Carlos", response.Nombre);
        Assert.AreEqual("Suarez", response.Apellido);
        Assert.AreEqual("carlos@farmeort.com", response.Email);
        Assert.AreEqual("carlossuarez", response.Nickname);
        Assert.AreEqual(new DateOnly(2001, 8, 2), response.FechaNacimiento);
        Assert.AreEqual(Roles.Jugador, response.Rol);
        Assert.AreEqual(1200, response.Elo);
    }

    [TestMethod]
    public void Listar_ConFiltro_DevuelveLosUsuariosDelRepositorio()
    {
        var carlos = new Usuario("Carlos", "Suarez", "carlos@farmeort.com", "carlossuarez", new DateOnly(2001, 8, 2), Rol.Jugador, "hash", FechaActual)
        {
            Id = 3,
        };
        _repositorioUsuarios.Setup(r => r.Listar(new FiltroUsuarios("car", null, null, 10, 0))).Returns([carlos]);

        var usuarios = _servicio.Listar(new FiltroUsuariosRequest { Nombre = "car", Limit = 10, Offset = 0 });

        Assert.HasCount(1, usuarios);
        Assert.AreEqual(3, usuarios[0].Id);
        Assert.AreEqual("Carlos", usuarios[0].Nombre);
        Assert.AreEqual(1200, usuarios[0].Elo);
    }

    [TestMethod]
    public void Listar_LimitMayorAlMaximo_UsaElMaximo()
    {
        _repositorioUsuarios.Setup(r => r.Listar(new FiltroUsuarios(null, null, null, 100, 0))).Returns([]);

        var usuarios = _servicio.Listar(new FiltroUsuariosRequest { Limit = 600 });

        Assert.IsEmpty(usuarios);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(-5)]
    public void Listar_LimitMenorAUno_UsaElValorPorDefecto(int limit)
    {
        _repositorioUsuarios.Setup(r => r.Listar(new FiltroUsuarios(null, null, null, 20, 0))).Returns([]);

        var usuarios = _servicio.Listar(new FiltroUsuariosRequest { Limit = limit });

        Assert.IsEmpty(usuarios);
    }

    [TestMethod]
    public void Listar_OffsetNegativo_EmpiezaDesdeElPrincipio()
    {
        _repositorioUsuarios.Setup(r => r.Listar(new FiltroUsuarios(null, null, null, 20, 0))).Returns([]);

        var usuarios = _servicio.Listar(new FiltroUsuariosRequest { Offset = -5 });

        Assert.IsEmpty(usuarios);
    }

    private void ConfigurarAltaValida()
    {
        _repositorioUsuarios.Setup(r => r.ObtenerPorEmail(Email)).Returns((Usuario?)null);
        _repositorioUsuarios.Setup(r => r.ExistePorNickname(Nickname)).Returns(false);
        _hasheadorContrasenas.Setup(h => h.Hashear(Contrasena)).Returns(ContrasenaHash);
        _reloj.Setup(r => r.GetUtcNow()).Returns(new DateTimeOffset(2026, 9, 30, 12, 0, 0, TimeSpan.Zero));
        _reloj.Setup(r => r.LocalTimeZone).Returns(TimeZoneInfo.Utc);
        _repositorioUsuarios.Setup(r => r.Agregar(It.Is<Usuario>(u => u.Email == Email)));
    }

    private static UsuarioAdminRequest CrearRequest(string rol = Roles.Organizador, string contrasena = Contrasena)
    {
        return new UsuarioAdminRequest
        {
            Nombre = "Ana",
            Apellido = "Pérez",
            Email = Email,
            Nickname = Nickname,
            Contrasena = contrasena,
            FechaNacimiento = new DateOnly(2000, 1, 15),
            Rol = rol,
        };
    }
}
