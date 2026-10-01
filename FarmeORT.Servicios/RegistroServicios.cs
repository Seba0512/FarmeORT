using FarmeORT.IServicios.Seguridad;
using FarmeORT.IServicios.Sesiones;
using FarmeORT.IServicios.Skills;
using FarmeORT.IServicios.Torneos;
using FarmeORT.IServicios.Usuarios;
using FarmeORT.Servicios.Seguridad;
using FarmeORT.Servicios.Sesiones;
using FarmeORT.Servicios.Skills;
using FarmeORT.Servicios.Torneos;
using FarmeORT.Servicios.Usuarios;
using Microsoft.Extensions.DependencyInjection;

namespace FarmeORT.Servicios;

public static class RegistroServicios
{
    public static IServiceCollection AgregarServicios(this IServiceCollection servicios)
    {
        servicios.AddScoped<IServicioSesiones, ServicioSesiones>();
        servicios.AddScoped<IHasheadorContrasenas, HasheadorContrasenas>();
        servicios.AddScoped<IGeneradorTokens, GeneradorTokens>();
        servicios.AddScoped<IServicioUsuarios, ServicioUsuarios>();
        servicios.AddScoped<IServicioSkills, ServicioSkills>();
        servicios.AddScoped<IServicioTorneos, ServicioTorneos>();
        return servicios;
    }
}
