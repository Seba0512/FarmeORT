using FarmeORT.DTOs.Sesiones;
using FarmeORT.IServicios.Sesiones;
using FarmeORT.WebAPI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FarmeORT.WebAPI.Tests.Controllers;

[TestClass]
public sealed class SesionesControllerTests
{
    private Mock<IServicioSesiones> _servicioSesiones = null!;
    private SesionesController _controller = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _servicioSesiones = new Mock<IServicioSesiones>(MockBehavior.Strict);
        _controller = new SesionesController(_servicioSesiones.Object);
    }

    [TestMethod]
    public void Crear_CredencialesValidas_Devuelve201ConLocationYSesion()
    {
        var request = new LoginRequest { Email = "jugador@farmeort.com", Contrasena = "Clave123!" };
        var sesion = new SesionResponse { Token = "token-opaco", UsuarioId = 7, Rol = "Jugador" };
        _servicioSesiones.Setup(s => s.IniciarSesion(request)).Returns(sesion);

        var resultado = _controller.Crear(request) as CreatedResult;

        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status201Created, resultado.StatusCode);
        Assert.AreEqual("/api/v1/sesiones/actual", resultado.Location);
        Assert.AreSame(sesion, resultado.Value);
        _servicioSesiones.Verify(s => s.IniciarSesion(request), Times.Once);
    }
}
