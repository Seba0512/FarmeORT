using System.Reflection;
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
        var sesion = new SesionResponse { Token = "3f2b8c1e-5d4a-4e7b-9c6f-1a2b3c4d5e6f", UsuarioId = 7, Rol = Roles.Jugador };
        _servicioSesiones.Setup(s => s.IniciarSesion(request)).Returns(sesion);

        var resultado = _controller.Crear(request) as CreatedResult;

        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status201Created, resultado.StatusCode);
        Assert.AreEqual("/api/v1/sesiones/3f2b8c1e-5d4a-4e7b-9c6f-1a2b3c4d5e6f", resultado.Location);
        Assert.AreSame(sesion, resultado.Value);
        _servicioSesiones.Verify(s => s.IniciarSesion(request), Times.Once);
    }

    [TestMethod]
    public void Eliminar_TokenDeLaRuta_Devuelve204YCierraEsaSesionDelUsuarioAutenticado()
    {
        var tokenDelEncabezado = Guid.Parse("3f2b8c1e-5d4a-4e7b-9c6f-1a2b3c4d5e6f");
        var tokenDeLaRuta = Guid.Parse("9a8b7c6d-5e4f-4a3b-8c2d-1e0f9a8b7c6d");
        var sesion = new SesionAutenticada { Token = tokenDelEncabezado, UsuarioId = 7, Rol = Roles.Jugador };
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        _controller.HttpContext.Items[ClavesContexto.SesionAutenticada] = sesion;
        _servicioSesiones.Setup(s => s.CerrarSesion(tokenDeLaRuta, 7));

        var resultado = _controller.Eliminar(tokenDeLaRuta);

        Assert.IsInstanceOfType<NoContentResult>(resultado);
        _servicioSesiones.Verify(s => s.CerrarSesion(tokenDeLaRuta, 7), Times.Once);
    }

    // El token identifica a la sesión en la ruta, como el id a cualquier otro recurso.
    [TestMethod]
    public void Eliminar_Ruta_IdentificaLaSesionPorSuToken()
    {
        var metodo = typeof(SesionesController).GetMethod(nameof(SesionesController.Eliminar))!;

        var verbo = metodo.GetCustomAttribute<HttpDeleteAttribute>();

        Assert.IsNotNull(verbo);
        Assert.AreEqual("{token:guid}", verbo.Template);
    }

    [TestMethod]
    public void Eliminar_CualquierRol_RequiereAutenticacionObligatoria()
    {
        var metodo = typeof(SesionesController).GetMethod(nameof(SesionesController.Eliminar))!;

        var atributo = metodo.GetCustomAttribute<AutenticacionAttribute>();

        Assert.IsNotNull(atributo);
        Assert.AreEqual(typeof(AutenticacionAttribute), atributo.GetType());
    }
}
