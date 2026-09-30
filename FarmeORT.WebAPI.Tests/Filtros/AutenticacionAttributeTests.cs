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
public sealed class AutenticacionAttributeTests
{
    private static readonly Guid Token = Guid.Parse("3f2b8c1e-5d4a-4e7b-9c6f-1a2b3c4d5e6f");

    private Mock<IServicioSesiones> _servicioSesiones = null!;
    private AutenticacionAttribute _filtro = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _servicioSesiones = new Mock<IServicioSesiones>(MockBehavior.Strict);
        _filtro = new AutenticacionAttribute();
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
        Assert.AreEqual("La sesión no es válida o expiró.", error.UserMessage);
    }

    [TestMethod]
    [DataRow("Basic dXN1YXJpbzpjbGF2ZQ==")]
    [DataRow("Bearer")]
    [DataRow("Bearer no-es-un-guid")]
    public void OnAuthorization_HeaderConFormatoInvalido_Devuelve401ConInvalidToken(string encabezado)
    {
        var contexto = CrearContexto(encabezado);

        _filtro.OnAuthorization(contexto);

        var resultado = contexto.Result as ObjectResult;
        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status401Unauthorized, resultado.StatusCode);
        var error = resultado.Value as ErrorResponse;
        Assert.IsNotNull(error);
        Assert.AreEqual("INVALID_TOKEN", error.ErrorCode);
    }

    [TestMethod]
    public void OnAuthorization_TokenInexistente_Devuelve401ConInvalidToken()
    {
        var contexto = CrearContexto($"Bearer {Token}");
        _servicioSesiones.Setup(s => s.ObtenerSesionAutenticada(Token)).Returns((SesionAutenticada?)null);

        _filtro.OnAuthorization(contexto);

        var resultado = contexto.Result as ObjectResult;
        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status401Unauthorized, resultado.StatusCode);
        var error = resultado.Value as ErrorResponse;
        Assert.IsNotNull(error);
        Assert.AreEqual("INVALID_TOKEN", error.ErrorCode);
    }

    [TestMethod]
    public void OnAuthorization_TokenValido_GuardaLaSesionYNoCortaElRequest()
    {
        var contexto = CrearContexto($"Bearer {Token}");
        var sesion = new SesionAutenticada { Token = Token, UsuarioId = 7, Rol = Roles.Jugador };
        _servicioSesiones.Setup(s => s.ObtenerSesionAutenticada(Token)).Returns(sesion);

        _filtro.OnAuthorization(contexto);

        Assert.IsNull(contexto.Result);
        Assert.AreSame(sesion, contexto.HttpContext.Items[ClavesContexto.SesionAutenticada]);
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
