using FarmeORT.IServicios.Seguridad;
using FarmeORT.IServicios.Sesiones;
using FarmeORT.Servicios.Seguridad;
using FarmeORT.Servicios.Sesiones;
using Microsoft.Extensions.DependencyInjection;

namespace FarmeORT.Servicios;

public static class RegistroServicios
{
    public static IServiceCollection AgregarServicios(this IServiceCollection servicios)
    {
        servicios.AddScoped<IServicioSesiones, ServicioSesiones>();
        servicios.AddScoped<IHasheadorContrasenas, HasheadorContrasenas>();
        servicios.AddScoped<IGeneradorTokens, GeneradorTokens>();
        return servicios;
    }
}
