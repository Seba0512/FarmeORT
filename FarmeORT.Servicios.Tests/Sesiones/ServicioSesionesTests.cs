using FarmeORT.Dominio.Usuarios;
using FarmeORT.DTOs.Sesiones;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Usuarios;
using FarmeORT.IServicios.Seguridad;
using FarmeORT.Servicios.Sesiones;
using Moq;

namespace FarmeORT.Servicios.Tests.Sesiones;

[TestClass]
public sealed class ServicioSesionesTests
{
    private const string Email = "jugador@farmeort.com";
    private const string ContrasenaHash = "hash-guardado";

    private Mock<IRepositorioUsuarios> _repositorioUsuarios = null!;
    private Mock<IHasheadorContrasenas> _hasheadorContrasenas = null!;
    private ServicioSesiones _servicio = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _repositorioUsuarios = new Mock<IRepositorioUsuarios>(MockBehavior.Strict);
        _hasheadorContrasenas = new Mock<IHasheadorContrasenas>(MockBehavior.Strict);
        _servicio = new ServicioSesiones(_repositorioUsuarios.Object, _hasheadorContrasenas.Object);
    }

    [TestMethod]
    public void IniciarSesion_EmailInexistente_LanzaCredencialesInvalidas()
    {
        var request = new LoginRequest { Email = Email, Contrasena = "Clave123!" };
        _repositorioUsuarios.Setup(r => r.ObtenerPorEmail(Email)).Returns((Usuario?)null);

        Assert.ThrowsExactly<CredencialesInvalidasException>(() => _servicio.IniciarSesion(request));
    }

    [TestMethod]
    public void IniciarSesion_ContrasenaIncorrecta_LanzaCredencialesInvalidas()
    {
        var request = new LoginRequest { Email = Email, Contrasena = "ClaveIncorrecta" };
        var usuario = new Usuario { ContrasenaHash = ContrasenaHash };
        _repositorioUsuarios.Setup(r => r.ObtenerPorEmail(Email)).Returns(usuario);
        _hasheadorContrasenas.Setup(h => h.Verificar("ClaveIncorrecta", ContrasenaHash)).Returns(false);

        Assert.ThrowsExactly<CredencialesInvalidasException>(() => _servicio.IniciarSesion(request));
    }
}
