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

    [TestMethod]
    public void OnException_BatallaNoCancelable_Devuelve409ConBATTLE_NOT_CANCELLABLE()
    {
        VerificarMapa(new BatallaNoCancelableException(), 409, "BATTLE_NOT_CANCELLABLE", "Solo se puede cancelar una batalla Pendiente o En arbitraje.");
    }

    [TestMethod]
    public void OnException_BatallaNoPendiente_Devuelve409ConBATTLE_NOT_PENDING()
    {
        VerificarMapa(new BatallaNoPendienteException(), 409, "BATTLE_NOT_PENDING", "Solo se puede lanzar una skill en una batalla Pendiente.");
    }

    [TestMethod]
    public void OnException_JugadorNoParticipa_Devuelve409ConPLAYER_NOT_IN_BATTLE()
    {
        VerificarMapa(new JugadorNoParticipaEnBatallaException(), 409, "PLAYER_NOT_IN_BATTLE", "El jugador no participa de esta batalla.");
    }

    [TestMethod]
    public void OnException_TurnoIncorrecto_Devuelve409ConNOT_PLAYER_TURN()
    {
        VerificarMapa(new TurnoIncorrectoException(), 409, "NOT_PLAYER_TURN", "No es tu turno para lanzar una skill.");
    }

    [TestMethod]
    public void OnException_SkillFueraDelRepertorio_Devuelve409ConSKILL_NOT_IN_REPERTOIRE()
    {
        VerificarMapa(new SkillFueraDelRepertorioException(), 409, "SKILL_NOT_IN_REPERTOIRE", "La skill no está en tu repertorio.");
    }

    [TestMethod]
    public void OnException_SkillYaLanzada_Devuelve409ConSKILL_ALREADY_THROWN()
    {
        VerificarMapa(new SkillYaLanzadaException(), 409, "SKILL_ALREADY_THROWN", "Ya lanzaste esta skill en la batalla.");
    }

    [TestMethod]
    public void OnException_BatallaModificada_Devuelve409ConBATTLE_CHANGED()
    {
        VerificarMapa(new BatallaModificadaException(), 409, "BATTLE_CHANGED", "La batalla cambió mientras se procesaba el pedido. Volvé a consultarla.");
    }

    [TestMethod]
    public void OnException_SkillObligatoriaPendiente_Devuelve409ConMANDATORY_SKILL_PENDING()
    {
        VerificarMapa(new SkillObligatoriaPendienteException(), 409, "MANDATORY_SKILL_PENDING", "Debés lanzar una skill obligatoria pendiente para completar tus cuatro turnos.");
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
