using FarmeORT.DTOs.Errores;
using FarmeORT.Excepciones;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FarmeORT.WebAPI.Filtros;

internal sealed class FiltroExcepciones : IExceptionFilter
{
    private static readonly (int Status, string ErrorCode, string UserMessage) ErrorInesperado =
        (StatusCodes.Status500InternalServerError, "INTERNAL_ERROR", "Ocurrió un error inesperado.");

    private static readonly Dictionary<Type, (int Status, string ErrorCode, string UserMessage)> Errores = new()
    {
        [typeof(CredencialesInvalidasException)] =
            (StatusCodes.Status401Unauthorized, "INVALID_CREDENTIALS", "Email o contraseña incorrectos."),
    };

    public void OnException(ExceptionContext context)
    {
        if(!Errores.TryGetValue(context.Exception.GetType(), out var errorMapeado))
        {
            errorMapeado = ErrorInesperado;
        }

        var (status, errorCode, userMessage) = errorMapeado;
        var error = new ErrorResponse { UserMessage = userMessage, ErrorCode = errorCode };
        context.Result = new ObjectResult(error) { StatusCode = status };
    }
}
