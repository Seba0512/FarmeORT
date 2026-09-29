using FarmeORT.Dominio.Usuarios;
using FarmeORT.DTOs.Sesiones;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Usuarios;
using FarmeORT.Servicios.Sesiones;
using Moq;

namespace FarmeORT.Servicios.Tests.Sesiones;

[TestClass]
public sealed class ServicioSesionesTests
{
    private const string Email = "jugador@farmeort.com";

    private Mock<IRepositorioUsuarios> _repositorioUsuarios = null!;
    private ServicioSesiones _servicio = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _repositorioUsuarios = new Mock<IRepositorioUsuarios>(MockBehavior.Strict);
        _servicio = new ServicioSesiones(_repositorioUsuarios.Object);
    }

    [TestMethod]
    public void IniciarSesion_EmailInexistente_LanzaCredencialesInvalidas()
    {
        var request = new LoginRequest { Email = Email, Contrasena = "Clave123!" };
        _repositorioUsuarios.Setup(r => r.ObtenerPorEmail(Email)).Returns((Usuario?)null);

        Assert.ThrowsExactly<CredencialesInvalidasException>(() => _servicio.IniciarSesion(request));
    }
}
