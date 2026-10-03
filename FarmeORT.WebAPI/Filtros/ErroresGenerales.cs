using FarmeORT.DTOs.Errores;

namespace FarmeORT.WebAPI.Filtros;

// El 500 lo arman FiltroExcepciones y el manejador global: tienen que responder lo mismo.
internal static class ErroresGenerales
{
    public const string CodigoInesperado = "INTERNAL_ERROR";

    public const string MensajeInesperado = "Ocurrió un error inesperado.";

    // Errores que el enrutamiento y MVC responden sin cuerpo.
    public static readonly IReadOnlyDictionary<int, ErrorResponse> SinCuerpo = new Dictionary<int, ErrorResponse>
    {
        [StatusCodes.Status404NotFound] =
            new() { ErrorCode = "NOT_FOUND", UserMessage = "No existe el recurso solicitado." },
        [StatusCodes.Status405MethodNotAllowed] =
            new() { ErrorCode = "METHOD_NOT_ALLOWED", UserMessage = "El recurso no admite esa operación." },
        [StatusCodes.Status415UnsupportedMediaType] =
            new() { ErrorCode = "UNSUPPORTED_MEDIA_TYPE", UserMessage = "El cuerpo tiene que enviarse en formato JSON." },
    };
}
