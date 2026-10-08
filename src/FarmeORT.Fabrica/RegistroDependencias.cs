using FarmeORT.Repositorios;
using FarmeORT.Servicios;
using Microsoft.Extensions.DependencyInjection;

namespace FarmeORT.Fabrica;

public static class RegistroDependencias
{
    public static IServiceCollection AgregarDependencias(this IServiceCollection servicios, string connectionString)
    {
        servicios.AddSingleton(TimeProvider.System);
        servicios.AgregarServicios();
        servicios.AgregarRepositorios(connectionString);
        return servicios;
    }
}
