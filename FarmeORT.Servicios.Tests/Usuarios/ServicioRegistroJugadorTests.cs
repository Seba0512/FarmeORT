using FarmeORT.Dominio.Usuarios;
using FarmeORT.DTOs.Usuarios;
using FarmeORT.IRepositorios.Usuarios;
using FarmeORT.IServicios.Seguridad;
using FarmeORT.Servicios.Usuarios;
using Moq;

namespace FarmeORT.Servicios.Tests.Usuarios;

[TestClass]
public sealed class ServicioRegistroJugadorTests
{
    private const string Email = "ana@farmeort.com";
    private const string Nickname = "anaperez";
    private const string Contrasena = "ClaveSegura123!!";
    private const string ContrasenaHash = "hash-generado";

    private Mock<IRepositorioUsuarios> _repositorio = null!;
    private Mock<IHasheadorContrasenas> _hasheador = null!;
    private Mock<TimeProvider> _reloj = null!;
    private ServicioUsuarios _servicio = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _repositorio = new Mock<IRepositorioUsuarios>(MockBehavior.Strict);
        _hasheador = new Mock<IHasheadorContrasenas>(MockBehavior.Strict);
        _reloj = new Mock<TimeProvider>(MockBehavior.Strict);
        _servicio = new ServicioUsuarios(_repositorio.Object, _hasheador.Object, _reloj.Object);
    }

    [TestMethod]
    public void RegistrarJugador_DatosValidos_GuardaJugadorHasheadoConEloInicial()
    {
        ConfigurarAltaValida();

        var response = _servicio.RegistrarJugador(CrearRequest());

        _repositorio.Verify(r => r.Agregar(It.Is<Usuario>(u =>
            u.Nombre == "Ana"
            && u.Apellido == "Pérez"
            && u.Email == Email
            && u.Nickname == Nickname
            && u.FechaNacimiento == new DateOnly(2000, 1, 15)
            && u.Rol == Rol.Jugador
            && u.ContrasenaHash == ContrasenaHash
            && u.Elo == 1200)), Times.Once);
        _hasheador.Verify(h => h.Hashear(Contrasena), Times.Once);
        Assert.AreEqual("Ana", response.Nombre);
        Assert.AreEqual("Pérez", response.Apellido);
        Assert.AreEqual(Email, response.Email);
        Assert.AreEqual(Nickname, response.Nickname);
        Assert.AreEqual(new DateOnly(2000, 1, 15), response.FechaNacimiento);
        Assert.AreEqual(Roles.Jugador, response.Rol);
        Assert.AreEqual(1200, response.Elo);
    }

    private void ConfigurarAltaValida()
    {
        _repositorio.Setup(r => r.ObtenerPorEmail(Email)).Returns((Usuario?)null);
        _repositorio.Setup(r => r.ExistePorNickname(Nickname)).Returns(false);
        _hasheador.Setup(h => h.Hashear(Contrasena)).Returns(ContrasenaHash);
        _reloj.Setup(r => r.GetUtcNow()).Returns(new DateTimeOffset(2026, 9, 30, 12, 0, 0, TimeSpan.Zero));
        _reloj.Setup(r => r.LocalTimeZone).Returns(TimeZoneInfo.Utc);
        _repositorio.Setup(r => r.Agregar(It.Is<Usuario>(u => u.Email == Email)));
    }

    private static RegistroJugadorRequest CrearRequest(DateOnly? fechaNacimiento = null)
    {
        return new RegistroJugadorRequest
        {
            Nombre = "Ana",
            Apellido = "Pérez",
            Email = Email,
            Nickname = Nickname,
            Contrasena = Contrasena,
            FechaNacimiento = fechaNacimiento ?? new DateOnly(2000, 1, 15),
        };
    }
}
