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

    private static UsuarioAdminRequest CrearRequest()
    {
        return new UsuarioAdminRequest
        {
            Nombre = "Ana",
            Apellido = "Pérez",
            Email = Email,
            Nickname = Nickname,
            Contrasena = "ClaveSegura123!!",
            FechaNacimiento = new DateOnly(2000, 1, 15),
            Rol = Roles.Organizador,
        };
    }
}
