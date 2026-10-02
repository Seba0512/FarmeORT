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
public sealed class FiltroExcepcionesFinalizacionTests
{
    [TestMethod]
    public void OnException_TorneoNoEnCurso_Devuelve409ConTOURNAMENT_NOT_IN_PROGRESS()
    {
        VerificarMapa(new TorneoNoEnCursoException(), 409, "TOURNAMENT_NOT_IN_PROGRESS", "El torneo no está en curso.");
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
