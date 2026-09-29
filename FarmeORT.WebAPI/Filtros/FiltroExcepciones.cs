using FarmeORT.DTOs.Errores;
using FarmeORT.Excepciones;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FarmeORT.WebAPI.Filtros;

internal sealed class FiltroExcepciones : IExceptionFilter
{
    private static readonly Dictionary<Type, (int Status, string ErrorCode, string UserMessage)> Errores = new()
    {
        [typeof(CredencialesInvalidasException)] =
            (StatusCodes.Status401Unauthorized, "INVALID_CREDENTIALS", "Email o contraseña incorrectos."),
    };

    public void OnException(ExceptionContext context)
    {
        var (status, errorCode, userMessage) = Errores[context.Exception.GetType()];
        var error = new ErrorResponse { UserMessage = userMessage, ErrorCode = errorCode };
        context.Result = new ObjectResult(error) { StatusCode = status };
    }
}
