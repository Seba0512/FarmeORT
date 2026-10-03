namespace FarmeORT.WebAPI.Filtros;

// El 500 lo arman FiltroExcepciones y el manejador global: tienen que responder lo mismo.
internal static class ErroresGenerales
{
    public const string CodigoInesperado = "INTERNAL_ERROR";

    public const string MensajeInesperado = "Ocurrió un error inesperado.";
}
