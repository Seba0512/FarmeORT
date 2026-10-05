using FarmeORT.Dominio.Batallas;
using FarmeORT.Dominio.Elo;
using FarmeORT.Dominio.Rondas;
using FarmeORT.IServicios.Batallas;
using FarmeORT.IServicios.Finalizaciones;
using FarmeORT.IServicios.Inscripciones;
using FarmeORT.IServicios.Ranking;
using FarmeORT.IServicios.Repertorio;
using FarmeORT.IServicios.Rondas;
using FarmeORT.IServicios.Seguridad;
using FarmeORT.IServicios.Sesiones;
using FarmeORT.IServicios.Skills;
using FarmeORT.IServicios.Torneos;
using FarmeORT.IServicios.Usuarios;
using FarmeORT.Servicios.Batallas;
using FarmeORT.Servicios.Finalizaciones;
using FarmeORT.Servicios.Inscripciones;
using FarmeORT.Servicios.Ranking;
using FarmeORT.Servicios.Repertorio;
using FarmeORT.Servicios.Rondas;
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
        // Seguridad y sesiones.
        servicios.AddScoped<IHasheadorContrasenas, HasheadorContrasenas>();
        servicios.AddScoped<IGeneradorTokens, GeneradorTokens>();
        servicios.AddScoped<IServicioSesiones, ServicioSesiones>();

        // Casos de uso.
        servicios.AddScoped<IServicioUsuarios, ServicioUsuarios>();
        servicios.AddScoped<IServicioSkills, ServicioSkills>();
        servicios.AddScoped<IServicioTorneos, ServicioTorneos>();
        servicios.AddScoped<IServicioRepertorio, ServicioRepertorio>();
        servicios.AddScoped<IServicioSkillsObligatorias, ServicioSkillsObligatorias>();
        servicios.AddScoped<IServicioInscripciones, ServicioInscripciones>();
        servicios.AddScoped<IServicioRondas, ServicioRondas>();
        servicios.AddScoped<IServicioBatallas, ServicioBatallas>();
        servicios.AddScoped<IServicioLanzamientos, ServicioLanzamientos>();
        servicios.AddScoped<IServicioFallos, ServicioFallos>();
        servicios.AddScoped<IServicioFinalizaciones, ServicioFinalizaciones>();
        servicios.AddScoped<IServicioRanking, ServicioRanking>();

        // Puntos de variación: otra implementación se registra acá sin tocar a quien la usa (D-51, D-60, D-62).
        servicios.AddScoped<ICalculadorElo, CalculadorEloBase>();
        servicios.AddScoped<IEstrategiaEmparejamiento, EmparejamientoPorElo>();
        servicios.AddScoped<ISorteo, SorteoAleatorio>();
        return servicios;
    }
}
