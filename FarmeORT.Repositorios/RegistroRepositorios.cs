using FarmeORT.IRepositorios.Sesiones;
using FarmeORT.IRepositorios.Usuarios;
using FarmeORT.Repositorios.Sesiones;
using FarmeORT.Repositorios.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FarmeORT.Repositorios;

public static class RegistroRepositorios
{
    public static IServiceCollection AgregarRepositorios(this IServiceCollection servicios, string connectionString)
    {
        servicios.AddDbContext<FarmeORTContext>(opciones => opciones.UseSqlServer(connectionString));
        servicios.AddScoped<IRepositorioUsuarios, RepositorioUsuarios>();
        servicios.AddScoped<IRepositorioSesiones, RepositorioSesiones>();
        return servicios;
    }
}
