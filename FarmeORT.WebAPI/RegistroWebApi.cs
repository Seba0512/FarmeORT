using FarmeORT.WebAPI.Filtros;
using FarmeORT.WebAPI.Validaciones;

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
    // responde el 500 del equipo, sin stack trace ni datos de la base. Los errores sin cuerpo reciben el suyo.
    public static IApplicationBuilder UsarManejoDeErrores(this IApplicationBuilder aplicacion)
    {
        aplicacion.UseExceptionHandler(errores => errores.Run(ErroresGenerales.ResponderErrorInesperado));
        aplicacion.UseStatusCodePages(ErroresGenerales.ResponderErrorSinCuerpo);
        return aplicacion;
    }
}
