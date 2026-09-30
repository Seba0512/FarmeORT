using FarmeORT.DTOs.Errores;
using FarmeORT.DTOs.Usuarios;
using FarmeORT.IServicios.Sesiones;
using FarmeORT.WebAPI.Filtros;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FarmeORT.WebAPI.Tests.Filtros;

[TestClass]
public sealed class AutorizacionAttributeTests
{
    private static readonly Guid Token = Guid.Parse("3f2b8c1e-5d4a-4e7b-9c6f-1a2b3c4d5e6f");

    private Mock<IServicioSesiones> _servicioSesiones = null!;
    private AutorizacionAttribute _filtro = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _servicioSesiones = new Mock<IServicioSesiones>(MockBehavior.Strict);
        _filtro = new AutorizacionAttribute(Roles.Administrador);
    }

    [TestMethod]
    public void OnAuthorization_RolNoPermitido_Devuelve403ConForbidden()
    {
        var contexto = ContextoDeFiltro.Crear(_servicioSesiones.Object, $"Bearer {Token}");
        var sesion = new SesionAutenticada { Token = Token, UsuarioId = 7, Rol = Roles.Jugador };
        _servicioSesiones.Setup(s => s.ObtenerSesionAutenticada(Token)).Returns(sesion);

        _filtro.OnAuthorization(contexto);

        var resultado = contexto.Result as ObjectResult;
        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status403Forbidden, resultado.StatusCode);
        var error = resultado.Value as ErrorResponse;
        Assert.IsNotNull(error);
        Assert.AreEqual("FORBIDDEN", error.ErrorCode);
        Assert.AreEqual("No tenés permisos para realizar esta operación.", error.UserMessage);
    }

    [TestMethod]
    public void OnAuthorization_RolPermitido_NoCortaElRequest()
    {
        var contexto = ContextoDeFiltro.Crear(_servicioSesiones.Object, $"Bearer {Token}");
        var sesion = new SesionAutenticada { Token = Token, UsuarioId = 1, Rol = Roles.Administrador };
        _servicioSesiones.Setup(s => s.ObtenerSesionAutenticada(Token)).Returns(sesion);

        _filtro.OnAuthorization(contexto);

        Assert.IsNull(contexto.Result);
        Assert.AreSame(sesion, contexto.HttpContext.Items[ClavesContexto.SesionAutenticada]);
    }

    [TestMethod]
    public void OnAuthorization_SinHeaderAuthorization_Devuelve401ConInvalidToken()
    {
        var contexto = ContextoDeFiltro.Crear(_servicioSesiones.Object);

        _filtro.OnAuthorization(contexto);

        var resultado = contexto.Result as ObjectResult;
        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status401Unauthorized, resultado.StatusCode);
        var error = resultado.Value as ErrorResponse;
        Assert.IsNotNull(error);
        Assert.AreEqual("INVALID_TOKEN", error.ErrorCode);
    }
}
