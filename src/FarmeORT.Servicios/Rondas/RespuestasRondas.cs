using FarmeORT.Dominio.Batallas;
using FarmeORT.DTOs.Rondas;
using FarmeORT.Servicios.Batallas;

namespace FarmeORT.Servicios.Rondas;

// Una ronda son las batallas del torneo con el mismo número (D-62). Lo usan las rondas y el cuadro del detalle del torneo.
internal static class RespuestasRondas
{
    public static RondaResponse Crear(int numero, IEnumerable<Batalla> batallas)
    {
        return new RondaResponse
        {
            Numero = numero,
            Batallas = batallas.Select(RespuestasBatallas.Resumen).ToList(),
        };
    }

    // El repositorio ya trae las batallas ordenadas por ronda y por id.
    public static IReadOnlyList<RondaResponse> Agrupar(IEnumerable<Batalla> batallas)
    {
        return batallas
            .GroupBy(b => b.NumeroRonda)
            .Select(ronda => Crear(ronda.Key, ronda))
            .ToList();
    }
}
