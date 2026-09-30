using FarmeORT.DTOs.Sesiones;
using FarmeORT.DTOs.Usuarios;
using FarmeORT.IServicios.Sesiones;
using FarmeORT.WebAPI.Controllers;
using FarmeORT.WebAPI.Filtros;
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

    [TestMethod]
    public void Eliminar_SesionAutenticada_Devuelve204YCierraLaSesion()
    {
        var token = Guid.Parse("3f2b8c1e-5d4a-4e7b-9c6f-1a2b3c4d5e6f");
        var sesion = new SesionAutenticada { Token = token, UsuarioId = 7, Rol = Roles.Jugador };
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        _controller.HttpContext.Items[ClavesContexto.SesionAutenticada] = sesion;
        _servicioSesiones.Setup(s => s.CerrarSesion(token));

        var resultado = _controller.Eliminar();

        Assert.IsInstanceOfType<NoContentResult>(resultado);
        _servicioSesiones.Verify(s => s.CerrarSesion(token), Times.Once);
    }
}
