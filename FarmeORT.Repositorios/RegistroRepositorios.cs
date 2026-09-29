using Microsoft.Extensions.DependencyInjection;

namespace FarmeORT.Repositorios;

public static class RegistroRepositorios
{
    public static IServiceCollection AgregarRepositorios(this IServiceCollection servicios, string connectionString)
    {
        _ = servicios;
        _ = connectionString;
        throw new NotImplementedException();
    }
}
