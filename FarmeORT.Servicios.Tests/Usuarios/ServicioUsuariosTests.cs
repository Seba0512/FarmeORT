using FarmeORT.Dominio.Usuarios;
using FarmeORT.DTOs.Usuarios;
using FarmeORT.Excepciones;
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
        _servicio = new ServicioUsuarios(_repositorioUsuarios.Object, _hasheadorContrasenas.Object, _reloj.Object);
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

    private void ConfigurarAltaValida()
    {
        _repositorioUsuarios.Setup(r => r.ObtenerPorEmail(Email)).Returns((Usuario?)null);
        _repositorioUsuarios.Setup(r => r.ExistePorNickname(Nickname)).Returns(false);
        _hasheadorContrasenas.Setup(h => h.Hashear(Contrasena)).Returns(ContrasenaHash);
        _reloj.Setup(r => r.GetUtcNow()).Returns(new DateTimeOffset(2026, 9, 30, 12, 0, 0, TimeSpan.Zero));
        _reloj.Setup(r => r.LocalTimeZone).Returns(TimeZoneInfo.Utc);
        _repositorioUsuarios.Setup(r => r.Agregar(It.Is<Usuario>(u => u.Email == Email)));
    }

    private static UsuarioAdminRequest CrearRequest(string rol = Roles.Organizador)
    {
        return new UsuarioAdminRequest
        {
            Nombre = "Ana",
            Apellido = "Pérez",
            Email = Email,
            Nickname = Nickname,
            Contrasena = Contrasena,
            FechaNacimiento = new DateOnly(2000, 1, 15),
            Rol = rol,
        };
    }
}
