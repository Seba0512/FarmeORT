using FarmeORT.IRepositorios.Inscripciones;
using FarmeORT.IRepositorios.Sesiones;
using FarmeORT.IRepositorios.Skills;
using FarmeORT.IRepositorios.Torneos;
using FarmeORT.IRepositorios.Usuarios;
using FarmeORT.Repositorios.Inscripciones;
using FarmeORT.Repositorios.Sesiones;
using FarmeORT.Repositorios.Skills;
using FarmeORT.Repositorios.Torneos;
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
        servicios.AddScoped<IRepositorioSkills, RepositorioSkills>();
        servicios.AddScoped<IRepositorioTorneos, RepositorioTorneos>();
        servicios.AddScoped<IRepositorioInscripciones, RepositorioInscripciones>();
        return servicios;
    }
}
