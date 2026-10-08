using FarmeORT.DTOs.Errores;
using Microsoft.AspNetCore.Diagnostics;

namespace FarmeORT.WebAPI.Filtros;

// Errores que no dependen de una regla: el 500 (lo arman FiltroExcepciones y el manejador global, tienen que
// responder lo mismo) y los que el enrutamiento y MVC responden sin cuerpo.
internal static class ErroresGenerales
{
    public const string CodigoInesperado = "INTERNAL_ERROR";

    public const string MensajeInesperado = "Ocurrió un error inesperado.";

    // Caso base para un error sin cuerpo que no está en la tabla.
    private static readonly ErrorResponse SinCuerpoNoPrevisto =
        new() { ErrorCode = "REQUEST_NOT_PROCESSED", UserMessage = "No se pudo procesar la solicitud." };

    private static readonly IReadOnlyDictionary<int, ErrorResponse> SinCuerpo = new Dictionary<int, ErrorResponse>
    {
        [StatusCodes.Status404NotFound] =
            new() { ErrorCode = "NOT_FOUND", UserMessage = "No existe el recurso solicitado." },
        [StatusCodes.Status405MethodNotAllowed] =
            new() { ErrorCode = "METHOD_NOT_ALLOWED", UserMessage = "El recurso no admite esa operación." },
        [StatusCodes.Status415UnsupportedMediaType] =
            new() { ErrorCode = "UNSUPPORTED_MEDIA_TYPE", UserMessage = "El cuerpo tiene que enviarse en formato JSON." },
    };

    public static Task ResponderErrorInesperado(HttpContext contexto)
    {
        var error = new ErrorResponse { UserMessage = MensajeInesperado, ErrorCode = CodigoInesperado };
        contexto.Response.StatusCode = StatusCodes.Status500InternalServerError;
        return contexto.Response.WriteAsJsonAsync(error);
    }

    // Una ruta inexistente o un método no admitido llegan sin cuerpo: se les agrega el error de la API.
    public static Task ResponderErrorSinCuerpo(StatusCodeContext contexto)
    {
        var respuesta = contexto.HttpContext.Response;
        var error = SinCuerpo.GetValueOrDefault(respuesta.StatusCode, SinCuerpoNoPrevisto);
        return respuesta.WriteAsJsonAsync(error);
    }
}
