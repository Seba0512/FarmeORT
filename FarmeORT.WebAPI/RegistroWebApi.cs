using FarmeORT.DTOs.Errores;
using FarmeORT.WebAPI.Filtros;
using FarmeORT.WebAPI.Validaciones;
using Microsoft.AspNetCore.Diagnostics;

namespace FarmeORT.WebAPI;

internal static class RegistroWebApi
{
    public static IServiceCollection AgregarControllersConFiltros(this IServiceCollection servicios)
    {
        servicios
            .AddControllers(opciones => opciones.Filters.Add<FiltroExcepciones>())
            .ConfigureApiBehaviorOptions(opciones =>
            {
                opciones.InvalidModelStateResponseFactory = RespuestaValidacion.Crear;

                // Sin ProblemDetails: esos errores llegan sin cuerpo y UsarManejoDeErrores les pone el del equipo.
                opciones.SuppressMapClientErrors = true;
            });
        return servicios;
    }

    // Lo que no atrapa FiltroExcepciones (por ejemplo, una falla de la base dentro del filtro de autenticación)
    // responde el 500 del equipo, sin stack trace ni datos de la base.
    public static IApplicationBuilder UsarManejoDeErrores(this IApplicationBuilder aplicacion)
    {
        aplicacion.UseExceptionHandler(errores => errores.Run(ResponderErrorInesperado));
        aplicacion.UseStatusCodePages(ResponderErrorSinCuerpo);
        return aplicacion;
    }

    // Una ruta inexistente o un método no admitido llegan sin cuerpo: se les agrega el error de la API.
    private static Task ResponderErrorSinCuerpo(StatusCodeContext contexto)
    {
        var respuesta = contexto.HttpContext.Response;
        var error = ErroresGenerales.SinCuerpo.GetValueOrDefault(respuesta.StatusCode, ErroresGenerales.SinCuerpoNoPrevisto);
        return respuesta.WriteAsJsonAsync(error);
    }

    private static Task ResponderErrorInesperado(HttpContext contexto)
    {
        var error = new ErrorResponse { UserMessage = ErroresGenerales.MensajeInesperado, ErrorCode = ErroresGenerales.CodigoInesperado };
        contexto.Response.StatusCode = StatusCodes.Status500InternalServerError;
        return contexto.Response.WriteAsJsonAsync(error);
    }
}
