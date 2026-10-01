using FarmeORT.Dominio.Sesiones;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.DTOs.Sesiones;
using FarmeORT.DTOs.Usuarios;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Sesiones;
using FarmeORT.IRepositorios.Usuarios;
using FarmeORT.IServicios.Seguridad;
using FarmeORT.Servicios.Sesiones;
using Moq;

namespace FarmeORT.Servicios.Tests.Sesiones;

[TestClass]
public sealed class ServicioSesionesTests
{
    private const string Email = "jugador@farmeort.com";
    private const string Contrasena = "Clave123!";
    private const string ContrasenaHash = "hash-guardado";
    private const int UsuarioId = 7;

    private static readonly Guid Token = Guid.Parse("3f2b8c1e-5d4a-4e7b-9c6f-1a2b3c4d5e6f");

    private Mock<IRepositorioUsuarios> _repositorioUsuarios = null!;
    private Mock<IRepositorioSesiones> _repositorioSesiones = null!;
    private Mock<IHasheadorContrasenas> _hasheadorContrasenas = null!;
    private Mock<IGeneradorTokens> _generadorTokens = null!;
    private ServicioSesiones _servicio = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _repositorioUsuarios = new Mock<IRepositorioUsuarios>(MockBehavior.Strict);
        _repositorioSesiones = new Mock<IRepositorioSesiones>(MockBehavior.Strict);
        _hasheadorContrasenas = new Mock<IHasheadorContrasenas>(MockBehavior.Strict);
        _generadorTokens = new Mock<IGeneradorTokens>(MockBehavior.Strict);
        _servicio = new ServicioSesiones(
            _repositorioUsuarios.Object,
            _repositorioSesiones.Object,
            _hasheadorContrasenas.Object,
            _generadorTokens.Object);
    }

    [TestMethod]
    public void IniciarSesion_EmailInexistente_LanzaCredencialesInvalidas()
    {
        var request = new LoginRequest { Email = Email, Contrasena = Contrasena };
        _repositorioUsuarios.Setup(r => r.ObtenerPorEmail(Email)).Returns((Usuario?)null);

        Assert.ThrowsExactly<CredencialesInvalidasException>(() => _servicio.IniciarSesion(request));
    }

    [TestMethod]
    public void IniciarSesion_ContrasenaIncorrecta_LanzaCredencialesInvalidas()
    {
        var request = new LoginRequest { Email = Email, Contrasena = "ClaveIncorrecta" };
        var usuario = CrearUsuario();
        _repositorioUsuarios.Setup(r => r.ObtenerPorEmail(Email)).Returns(usuario);
        _hasheadorContrasenas.Setup(h => h.Verificar("ClaveIncorrecta", ContrasenaHash)).Returns(false);

        Assert.ThrowsExactly<CredencialesInvalidasException>(() => _servicio.IniciarSesion(request));
    }

    [TestMethod]
    public void IniciarSesion_UsuarioEliminadoConContrasenaCorrecta_LanzaCredencialesInvalidasSinCrearSesion()
    {
        var request = new LoginRequest { Email = Email, Contrasena = Contrasena };
        var usuario = CrearUsuario();
        usuario.Eliminar();
        _repositorioUsuarios.Setup(r => r.ObtenerPorEmail(Email)).Returns(usuario);
        _hasheadorContrasenas.Setup(h => h.Verificar(Contrasena, ContrasenaHash)).Returns(true);
        _generadorTokens.Setup(g => g.Generar()).Returns(Token);
        _repositorioSesiones.Setup(r => r.Agregar(It.Is<Sesion>(s => s.Token == Token)));

        Assert.ThrowsExactly<CredencialesInvalidasException>(() => _servicio.IniciarSesion(request));

        _repositorioSesiones.Verify(r => r.Agregar(It.IsAny<Sesion>()), Times.Never);
    }

    [TestMethod]
    public void IniciarSesion_CredencialesValidas_DevuelveTokenUsuarioYRol()
    {
        var request = new LoginRequest { Email = Email, Contrasena = Contrasena };
        ConfigurarCredencialesValidas();

        var response = _servicio.IniciarSesion(request);

        Assert.AreEqual(Token.ToString(), response.Token);
        Assert.AreEqual(UsuarioId, response.UsuarioId);
        Assert.AreEqual("Jugador", response.Rol);
    }

    [TestMethod]
    public void IniciarSesion_CredencialesValidas_GuardaSesion()
    {
        var request = new LoginRequest { Email = Email, Contrasena = Contrasena };
        var usuario = ConfigurarCredencialesValidas();

        _servicio.IniciarSesion(request);

        _repositorioSesiones.Verify(
            r => r.Agregar(It.Is<Sesion>(s => s.Token == Token && s.Usuario == usuario)),
            Times.Once);
    }

    [TestMethod]
    public void CerrarSesion_Token_EliminaLaSesion()
    {
        _repositorioSesiones.Setup(r => r.Eliminar(Token));

        _servicio.CerrarSesion(Token);

        _repositorioSesiones.Verify(r => r.Eliminar(Token), Times.Once);
    }

    [TestMethod]
    public void ObtenerSesionAutenticada_TokenSinSesion_DevuelveNull()
    {
        _repositorioSesiones.Setup(r => r.ObtenerPorToken(Token)).Returns((Sesion?)null);

        var resultado = _servicio.ObtenerSesionAutenticada(Token);

        Assert.IsNull(resultado);
    }

    [TestMethod]
    public void ObtenerSesionAutenticada_TokenConSesion_DevuelveTokenUsuarioYRol()
    {
        var usuario = CrearUsuario();
        _repositorioSesiones.Setup(r => r.ObtenerPorToken(Token)).Returns(new Sesion(Token, usuario));

        var resultado = _servicio.ObtenerSesionAutenticada(Token);

        Assert.IsNotNull(resultado);
        Assert.AreEqual(Token, resultado.Token);
        Assert.AreEqual(UsuarioId, resultado.UsuarioId);
        Assert.AreEqual(Roles.Jugador, resultado.Rol);
    }

    private Usuario ConfigurarCredencialesValidas()
    {
        var usuario = CrearUsuario();
        _repositorioUsuarios.Setup(r => r.ObtenerPorEmail(Email)).Returns(usuario);
        _hasheadorContrasenas.Setup(h => h.Verificar(Contrasena, ContrasenaHash)).Returns(true);
        _generadorTokens.Setup(g => g.Generar()).Returns(Token);
        _repositorioSesiones.Setup(r => r.Agregar(It.Is<Sesion>(s => s.Token == Token && s.Usuario == usuario)));
        return usuario;
    }

    private static Usuario CrearUsuario()
    {
        return new Usuario("Ana", "Pérez", Email, "anaperez", new DateOnly(2000, 1, 15), Rol.Jugador, ContrasenaHash, new DateOnly(2026, 9, 30))
        {
            Id = UsuarioId,
        };
    }
}
