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
public sealed class FiltroExcepcionesRondasTests
{
    [TestMethod]
    public void OnException_TorneoModificado_Devuelve409ConTOURNAMENT_CHANGED()
    {
        VerificarMapa(new TorneoModificadoException(), 409, "TOURNAMENT_CHANGED", "El torneo cambió mientras se procesaba el pedido. Volvé a consultarlo.");
    }

    [TestMethod]
    public void OnException_TorneoIncompleto_Devuelve409ConTOURNAMENT_NOT_FULL()
    {
        VerificarMapa(new TorneoIncompletoException(), 409, "TOURNAMENT_NOT_FULL", "El torneo todavía no completó su cupo de inscripciones aceptadas.");
    }

    [TestMethod]
    public void OnException_RondaNoEncontrada_Devuelve404ConROUND_NOT_FOUND()
    {
        VerificarMapa(new RondaNoEncontradaException(), 404, "ROUND_NOT_FOUND", "El torneo no tiene una ronda con ese número.");
    }

    [TestMethod]
    public void OnException_RondaSinResolver_Devuelve409ConROUND_NOT_RESOLVED()
    {
        VerificarMapa(new RondaSinResolverException(), 409, "ROUND_NOT_RESOLVED", "La ronda anterior todavía tiene batallas sin resolver.");
    }

    [TestMethod]
    public void OnException_TorneoListoParaFinalizar_Devuelve409ConTOURNAMENT_READY_TO_FINISH()
    {
        VerificarMapa(new TorneoListoParaFinalizarException(), 409, "TOURNAMENT_READY_TO_FINISH", "Queda un solo jugador sin eliminar: corresponde finalizar el torneo.");
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
