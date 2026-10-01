using Microsoft.Extensions.DependencyInjection;

namespace FarmeORT.Fabrica;

public static class RegistroDependencias
{
    public static IServiceCollection AgregarDependencias(this IServiceCollection servicios, string connectionString)
    {
        _ = servicios;
        _ = connectionString;
        throw new NotImplementedException();
    }
}
