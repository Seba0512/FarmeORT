using FarmeORT.DTOs.Errores;
using FarmeORT.DTOs.Usuarios;
using FarmeORT.IServicios.Sesiones;
using FarmeORT.WebAPI.Filtros;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
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
        var contexto = CrearContexto($"Bearer {Token}");
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
        var contexto = CrearContexto($"Bearer {Token}");
        var sesion = new SesionAutenticada { Token = Token, UsuarioId = 1, Rol = Roles.Administrador };
        _servicioSesiones.Setup(s => s.ObtenerSesionAutenticada(Token)).Returns(sesion);

        _filtro.OnAuthorization(contexto);

        Assert.IsNull(contexto.Result);
        Assert.AreSame(sesion, contexto.HttpContext.Items[ClavesContexto.SesionAutenticada]);
    }

    [TestMethod]
    public void OnAuthorization_SinHeaderAuthorization_Devuelve401ConInvalidToken()
    {
        var contexto = CrearContexto();

        _filtro.OnAuthorization(contexto);

        var resultado = contexto.Result as ObjectResult;
        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status401Unauthorized, resultado.StatusCode);
        var error = resultado.Value as ErrorResponse;
        Assert.IsNotNull(error);
        Assert.AreEqual("INVALID_TOKEN", error.ErrorCode);
    }

    private AuthorizationFilterContext CrearContexto(string? encabezadoAuthorization = null)
    {
        var servicios = new ServiceCollection().AddSingleton(_servicioSesiones.Object).BuildServiceProvider();
        var contextoHttp = new DefaultHttpContext { RequestServices = servicios };
        if(encabezadoAuthorization is not null)
        {
            contextoHttp.Request.Headers.Authorization = encabezadoAuthorization;
        }

        var contextoAccion = new ActionContext(contextoHttp, new RouteData(), new ActionDescriptor());
        return new AuthorizationFilterContext(contextoAccion, []);
    }
}
