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
    [DataRow(typeof(EmailYaExisteException), "EMAIL_ALREADY_EXISTS", "Ya existe un usuario con ese email.")]
    [DataRow(typeof(NicknameYaExisteException), "NICKNAME_ALREADY_EXISTS", "Ya existe un usuario con ese nickname.")]
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
