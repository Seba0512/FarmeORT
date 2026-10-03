using FarmeORT.WebAPI.Filtros;
using FarmeORT.WebAPI.Validaciones;

namespace FarmeORT.WebAPI;

internal static class RegistroWebApi
{
    public static IServiceCollection AgregarControllersConFiltros(this IServiceCollection servicios)
    {
        servicios
            .AddControllers(opciones => opciones.Filters.Add<FiltroExcepciones>())
            .ConfigureApiBehaviorOptions(opciones => opciones.InvalidModelStateResponseFactory = RespuestaValidacion.Crear);
        return servicios;
    }

    public static IApplicationBuilder UsarManejoDeErrores(this IApplicationBuilder aplicacion)
    {
        throw new NotImplementedException();
    }
}
