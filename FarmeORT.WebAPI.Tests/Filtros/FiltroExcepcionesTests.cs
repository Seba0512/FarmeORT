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
public sealed class FiltroExcepcionesTests
{
    private FiltroExcepciones _filtro = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _filtro = new FiltroExcepciones();
    }

    [TestMethod]
    public void OnException_CredencialesInvalidas_Devuelve401ConErrorInvalidCredentials()
    {
        var contexto = CrearContexto(new CredencialesInvalidasException());

        _filtro.OnException(contexto);

        var resultado = contexto.Result as ObjectResult;
        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status401Unauthorized, resultado.StatusCode);
        var error = resultado.Value as ErrorResponse;
        Assert.IsNotNull(error);
        Assert.AreEqual("INVALID_CREDENTIALS", error.ErrorCode);
        Assert.AreEqual("Email o contraseña incorrectos.", error.UserMessage);
    }

    [TestMethod]
    public void OnException_UsuarioNoEncontrado_Devuelve404ConUserNotFound()
    {
        var contexto = CrearContexto(new UsuarioNoEncontradoException());

        _filtro.OnException(contexto);

        var resultado = contexto.Result as ObjectResult;
        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status404NotFound, resultado.StatusCode);
        var error = resultado.Value as ErrorResponse;
        Assert.IsNotNull(error);
        Assert.AreEqual("USER_NOT_FOUND", error.ErrorCode);
        Assert.AreEqual("No existe un usuario con ese id.", error.UserMessage);
    }

    [TestMethod]
    public void OnException_SkillNoEncontrada_Devuelve404ConSkillNotFound()
    {
        var contexto = CrearContexto(new SkillNoEncontradaException());

        _filtro.OnException(contexto);

        var resultado = contexto.Result as ObjectResult;
        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status404NotFound, resultado.StatusCode);
        var error = resultado.Value as ErrorResponse;
        Assert.IsNotNull(error);
        Assert.AreEqual("SKILL_NOT_FOUND", error.ErrorCode);
        Assert.AreEqual("No existe una skill con ese id.", error.UserMessage);
    }

    [TestMethod]
    [DataRow(typeof(TorneoNoEncontradoException), StatusCodes.Status404NotFound, "TOURNAMENT_NOT_FOUND", "No existe un torneo con ese id.")]
    [DataRow(typeof(CodigoTorneoYaExisteException), StatusCodes.Status409Conflict, "TOURNAMENT_CODE_ALREADY_EXISTS", "Ya existe un torneo con ese código.")]
    [DataRow(typeof(TorneoNoModificableException), StatusCodes.Status409Conflict, "TOURNAMENT_NOT_MODIFIABLE", "Solo se puede modificar un torneo Abierto.")]
    [DataRow(typeof(TorneoNoCancelableException), StatusCodes.Status409Conflict, "TOURNAMENT_NOT_CANCELLABLE", "Solo se puede cancelar un torneo Abierto o En curso.")]
    public void OnException_ErrorDeTorneo_DevuelveSuStatusYCodigo(Type tipo, int status, string codigo, string mensaje)
    {
        var contexto = CrearContexto((Exception)Activator.CreateInstance(tipo)!);

        _filtro.OnException(contexto);

        var resultado = contexto.Result as ObjectResult;
        Assert.IsNotNull(resultado);
        Assert.AreEqual(status, resultado.StatusCode);
        var error = resultado.Value as ErrorResponse;
        Assert.IsNotNull(error);
        Assert.AreEqual(codigo, error.ErrorCode);
        Assert.AreEqual(mensaje, error.UserMessage);
    }

    [TestMethod]
    public void OnException_DatosInvalidos_Devuelve400ConElMensajeDeLaRegla()
    {
        var contexto = CrearContexto(new DatosInvalidosException("El usuario debe tener 13 años o más."));

        _filtro.OnException(contexto);

        var resultado = contexto.Result as ObjectResult;
        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status400BadRequest, resultado.StatusCode);
        var error = resultado.Value as ErrorResponse;
        Assert.IsNotNull(error);
        Assert.AreEqual("INVALID_DATA", error.ErrorCode);
        Assert.AreEqual("El usuario debe tener 13 años o más.", error.UserMessage);
    }

    [TestMethod]
    [DataRow(typeof(EmailYaExisteException), "EMAIL_ALREADY_EXISTS", "Ya existe un usuario con ese email.")]
    [DataRow(typeof(NicknameYaExisteException), "NICKNAME_ALREADY_EXISTS", "Ya existe un usuario con ese nickname.")]
    [DataRow(typeof(CodigoSkillYaExisteException), "SKILL_CODE_ALREADY_EXISTS", "Ya existe una skill con ese código.")]
    public void OnException_DatoDuplicado_Devuelve409ConSuCodigo(Type tipoExcepcion, string codigo, string mensaje)
    {
        var excepcion = (Exception)Activator.CreateInstance(tipoExcepcion)!;
        var contexto = CrearContexto(excepcion);

        _filtro.OnException(contexto);

        var resultado = contexto.Result as ObjectResult;
        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status409Conflict, resultado.StatusCode);
        var error = resultado.Value as ErrorResponse;
        Assert.IsNotNull(error);
        Assert.AreEqual(codigo, error.ErrorCode);
        Assert.AreEqual(mensaje, error.UserMessage);
    }

    [TestMethod]
    public void OnException_OperacionSobreSiMismo_Devuelve409ConSelfOperationNotAllowed()
    {
        var contexto = CrearContexto(new OperacionSobreSiMismoException());

        _filtro.OnException(contexto);

        var resultado = contexto.Result as ObjectResult;
        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status409Conflict, resultado.StatusCode);
        var error = resultado.Value as ErrorResponse;
        Assert.IsNotNull(error);
        Assert.AreEqual("SELF_OPERATION_NOT_ALLOWED", error.ErrorCode);
        Assert.AreEqual("Un usuario no puede modificarse ni eliminarse a sí mismo.", error.UserMessage);
    }

    [TestMethod]
    public void OnException_ExcepcionNoMapeada_Devuelve500ConMensajeFijo()
    {
        var contexto = CrearContexto(new InvalidOperationException("detalle interno con datos sensibles"));

        _filtro.OnException(contexto);

        var resultado = contexto.Result as ObjectResult;
        Assert.IsNotNull(resultado);
        Assert.AreEqual(StatusCodes.Status500InternalServerError, resultado.StatusCode);
        var error = resultado.Value as ErrorResponse;
        Assert.IsNotNull(error);
        Assert.AreEqual("INTERNAL_ERROR", error.ErrorCode);
        Assert.AreEqual("Ocurrió un error inesperado.", error.UserMessage);
        Assert.IsNull(error.MoreInfo);
        Assert.IsNull(error.Detalles);
    }

    private static ExceptionContext CrearContexto(Exception excepcion)
    {
        var contextoAccion = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
        return new ExceptionContext(contextoAccion, []) { Exception = excepcion };
    }
}
