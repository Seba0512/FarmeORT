using FarmeORT.DTOs.Errores;
using FarmeORT.Excepciones;
using FarmeORT.WebAPI.Filtros;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace FarmeORT.WebAPI.Tests.Filtros;

[TestClass]
public sealed class FiltroExcepcionesBatallasTests
{
    [TestMethod]
    public void OnException_BatallaNoEncontrada_Devuelve404ConBATTLE_NOT_FOUND()
    {
        VerificarMapa(new BatallaNoEncontradaException(), 404, "BATTLE_NOT_FOUND", "No existe una batalla con ese id.");
    }

    private static void VerificarMapa(Exception excepcion, int status, string codigo, string mensaje)
    {
        var accion = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
        var contexto = new ExceptionContext(accion, []) { Exception = excepcion };
        var filtro = new FiltroExcepciones();

        filtro.OnException(contexto);

        var resultado = contexto.Result as ObjectResult;
        Assert.IsNotNull(resultado);
        Assert.AreEqual(status, resultado.StatusCode);
        var error = resultado.Value as ErrorResponse;
        Assert.IsNotNull(error);
        Assert.AreEqual(codigo, error.ErrorCode);
        Assert.AreEqual(mensaje, error.UserMessage);
    }
}
