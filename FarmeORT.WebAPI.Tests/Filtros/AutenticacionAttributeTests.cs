using FarmeORT.DTOs.Errores;
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

    private AuthorizationFilterContext CrearContexto()
    {
        var servicios = new ServiceCollection().AddSingleton(_servicioSesiones.Object).BuildServiceProvider();
        var contextoHttp = new DefaultHttpContext { RequestServices = servicios };
        var contextoAccion = new ActionContext(contextoHttp, new RouteData(), new ActionDescriptor());
        return new AuthorizationFilterContext(contextoAccion, []);
    }
}
