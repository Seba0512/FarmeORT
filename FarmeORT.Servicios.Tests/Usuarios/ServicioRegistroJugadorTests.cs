using FarmeORT.Dominio.Usuarios;
using FarmeORT.DTOs.Usuarios;
using FarmeORT.Excepciones;
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

    [TestMethod]
    public void RegistrarJugador_FechaNacimientoAusente_LanzaDatosInvalidosSinGuardar()
    {
        ConfigurarAltaValida();
        var request = new RegistroJugadorRequest
        {
            Nombre = "Ana",
            Apellido = "Pérez",
            Email = Email,
            Nickname = Nickname,
            Contrasena = Contrasena,
        };

        Assert.ThrowsExactly<DatosInvalidosException>(() => _servicio.RegistrarJugador(request));
        _repositorio.Verify(r => r.Agregar(It.IsAny<Usuario>()), Times.Never);
        _hasheador.Verify(h => h.Hashear(Contrasena), Times.Never);
    }

    [TestMethod]
    public void RegistrarJugador_EmailExistente_LanzaEmailYaExisteSinGuardarNiHashear()
    {
        var existente = new Usuario("Otra", "Persona", Email, "otrapersona", new DateOnly(2000, 1, 15), Rol.Organizador, "hash", new DateOnly(2026, 9, 30));
        _repositorio.Setup(r => r.ObtenerPorEmail(Email)).Returns(existente);

        Assert.ThrowsExactly<EmailYaExisteException>(() => _servicio.RegistrarJugador(CrearRequest()));
        _repositorio.Verify(r => r.Agregar(It.IsAny<Usuario>()), Times.Never);
        _hasheador.Verify(h => h.Hashear(Contrasena), Times.Never);
    }

    [TestMethod]
    public void RegistrarJugador_NicknameExistente_LanzaNicknameYaExisteSinGuardarNiHashear()
    {
        _repositorio.Setup(r => r.ObtenerPorEmail(Email)).Returns((Usuario?)null);
        _repositorio.Setup(r => r.ExistePorNickname(Nickname)).Returns(true);

        Assert.ThrowsExactly<NicknameYaExisteException>(() => _servicio.RegistrarJugador(CrearRequest()));
        _repositorio.Verify(r => r.Agregar(It.IsAny<Usuario>()), Times.Never);
        _hasheador.Verify(h => h.Hashear(Contrasena), Times.Never);
    }

    [TestMethod]
    public void RegistrarJugador_ContrasenaInvalida_LanzaDatosInvalidosSinConsultarLaBase()
    {
        var request = CrearRequest(contrasena: "corta");

        Assert.ThrowsExactly<DatosInvalidosException>(() => _servicio.RegistrarJugador(request));
        _repositorio.Verify(r => r.Agregar(It.IsAny<Usuario>()), Times.Never);
    }

    [TestMethod]
    [DataRow("nombre", "")]
    [DataRow("apellido", "Pe")]
    [DataRow("email", "no-es-email")]
    [DataRow("nickname", "con espacio")]
    public void RegistrarJugador_DatosPersonalesInvalidos_LanzaDatosInvalidosSinGuardar(string campo, string valor)
    {
        var request = new RegistroJugadorRequest
        {
            Nombre = campo == "nombre" ? valor : "Ana",
            Apellido = campo == "apellido" ? valor : "Pérez",
            Email = campo == "email" ? valor : Email,
            Nickname = campo == "nickname" ? valor : Nickname,
            Contrasena = Contrasena,
            FechaNacimiento = new DateOnly(2000, 1, 15),
        };
        _repositorio.Setup(r => r.ObtenerPorEmail(request.Email)).Returns((Usuario?)null);
        _repositorio.Setup(r => r.ExistePorNickname(request.Nickname)).Returns(false);
        _hasheador.Setup(h => h.Hashear(Contrasena)).Returns(ContrasenaHash);
        ConfigurarReloj();

        Assert.ThrowsExactly<DatosInvalidosException>(() => _servicio.RegistrarJugador(request));
        _repositorio.Verify(r => r.Agregar(It.IsAny<Usuario>()), Times.Never);
    }

    [TestMethod]
    [DataRow(2013, 10, 1)]
    [DataRow(2027, 1, 1)]
    [DataRow(9999, 12, 31)]
    public void RegistrarJugador_NoCumpleEdadMinima_LanzaDatosInvalidosSinGuardar(int anio, int mes, int dia)
    {
        ConfigurarAltaValida();

        Assert.ThrowsExactly<DatosInvalidosException>(() => _servicio.RegistrarJugador(CrearRequest(new DateOnly(anio, mes, dia))));
        _repositorio.Verify(r => r.Agregar(It.IsAny<Usuario>()), Times.Never);
    }

    [TestMethod]
    public void RegistrarJugador_Cumple13Hoy_DevuelveJugador()
    {
        ConfigurarAltaValida();

        var response = _servicio.RegistrarJugador(CrearRequest(new DateOnly(2013, 9, 30)));

        Assert.AreEqual(new DateOnly(2013, 9, 30), response.FechaNacimiento);
        Assert.AreEqual(Roles.Jugador, response.Rol);
    }

    [TestMethod]
    public void RegistrarJugador_Cumple13ManianaEnFechaLocal_LanzaDatosInvalidos()
    {
        ConfigurarAltaValida();
        _reloj.Setup(r => r.GetUtcNow()).Returns(new DateTimeOffset(2026, 10, 1, 1, 0, 0, TimeSpan.Zero));
        _reloj.Setup(r => r.LocalTimeZone).Returns(TimeZoneInfo.CreateCustomTimeZone("MontevideoTest", TimeSpan.FromHours(-3), "Montevideo", "Montevideo"));

        Assert.ThrowsExactly<DatosInvalidosException>(() => _servicio.RegistrarJugador(CrearRequest(new DateOnly(2013, 10, 1))));
        _repositorio.Verify(r => r.Agregar(It.IsAny<Usuario>()), Times.Never);
    }

    private void ConfigurarAltaValida()
    {
        _repositorio.Setup(r => r.ObtenerPorEmail(Email)).Returns((Usuario?)null);
        _repositorio.Setup(r => r.ExistePorNickname(Nickname)).Returns(false);
        _hasheador.Setup(h => h.Hashear(Contrasena)).Returns(ContrasenaHash);
        ConfigurarReloj();
        _repositorio.Setup(r => r.Agregar(It.Is<Usuario>(u => u.Email == Email)));
    }

    private void ConfigurarReloj()
    {
        _reloj.Setup(r => r.GetUtcNow()).Returns(new DateTimeOffset(2026, 9, 30, 12, 0, 0, TimeSpan.Zero));
        _reloj.Setup(r => r.LocalTimeZone).Returns(TimeZoneInfo.Utc);
    }

    private static RegistroJugadorRequest CrearRequest(DateOnly? fechaNacimiento = null, string contrasena = Contrasena)
    {
        return new RegistroJugadorRequest
        {
            Nombre = "Ana",
            Apellido = "Pérez",
            Email = Email,
            Nickname = Nickname,
            Contrasena = contrasena,
            FechaNacimiento = fechaNacimiento ?? new DateOnly(2000, 1, 15),
        };
    }
}
