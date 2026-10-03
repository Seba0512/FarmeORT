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
        servicios.AddScoped<IServicioSesiones, ServicioSesiones>();
        servicios.AddScoped<IHasheadorContrasenas, HasheadorContrasenas>();
        servicios.AddScoped<IGeneradorTokens, GeneradorTokens>();
        servicios.AddScoped<IServicioUsuarios, ServicioUsuarios>();
        servicios.AddScoped<IServicioSkills, ServicioSkills>();
        servicios.AddScoped<IServicioTorneos, ServicioTorneos>();
        servicios.AddScoped<IServicioRepertorio, ServicioRepertorio>();
        servicios.AddScoped<IServicioSkillsObligatorias, ServicioSkillsObligatorias>();
        servicios.AddScoped<IServicioInscripciones, ServicioInscripciones>();

        servicios.AddScoped<ICalculadorElo, CalculadorEloBase>();

        // Persona B: batallas y lanzamientos.
        servicios.AddScoped<ISorteo, SorteoAleatorio>();
        servicios.AddScoped<IServicioLanzamientos, ServicioLanzamientos>();

        servicios.AddScoped<IServicioRanking, ServicioRanking>();

        // Persona B: fallos de batallas.
        servicios.AddScoped<IServicioFallos, ServicioFallos>();

        // Persona B: consultas de batallas.
        servicios.AddScoped<IServicioBatallas, ServicioBatallas>();

        // Persona C: rondas y su estrategia de emparejamiento (D-62).
        servicios.AddScoped<IEstrategiaEmparejamiento, EmparejamientoPorElo>();
        servicios.AddScoped<IServicioRondas, ServicioRondas>();

        // Persona C: finalización del torneo (D-64).
        servicios.AddScoped<IServicioFinalizaciones, ServicioFinalizaciones>();
        return servicios;
    }
}
