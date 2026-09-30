using FarmeORT.DTOs.Errores;
using FarmeORT.Excepciones;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FarmeORT.WebAPI.Filtros;

internal sealed class FiltroExcepciones : IExceptionFilter
{
    private static readonly ErrorMapeado ErrorInesperado =
        new(StatusCodes.Status500InternalServerError, "INTERNAL_ERROR", "Ocurrió un error inesperado.");

    private static readonly Dictionary<Type, ErrorMapeado> Errores = new()
    {
        [typeof(CredencialesInvalidasException)] =
            new(StatusCodes.Status401Unauthorized, "INVALID_CREDENTIALS", "Email o contraseña incorrectos."),
    };

    public void OnException(ExceptionContext context)
    {
        var errorMapeado = Errores.GetValueOrDefault(context.Exception.GetType(), ErrorInesperado);
        var error = new ErrorResponse { UserMessage = errorMapeado.UserMessage, ErrorCode = errorMapeado.ErrorCode };
        context.Result = new ObjectResult(error) { StatusCode = errorMapeado.Status };
    }

    private sealed record ErrorMapeado(int Status, string ErrorCode, string UserMessage);
}
