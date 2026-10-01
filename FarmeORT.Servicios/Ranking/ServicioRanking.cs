using FarmeORT.DTOs.Ranking;
using FarmeORT.IRepositorios.Usuarios;
using FarmeORT.IServicios.Ranking;

namespace FarmeORT.Servicios.Ranking;

internal sealed class ServicioRanking(IRepositorioUsuarios repositorioUsuarios) : IServicioRanking
{
    private const int CantidadMaxima = 100;
    private const int CantidadPorDefecto = 10;

    public IReadOnlyList<JugadorRankingResponse> Obtener(FiltroRankingRequest filtro)
    {
        var jugadores = repositorioUsuarios.ListarRanking(AjustarCantidad(filtro.Cantidad));
        List<JugadorRankingResponse> ranking = [];
        for(var i = 0; i < jugadores.Count; i++)
        {
            // Las batallas ganadas y perdidas quedan en 0 hasta que exista Batalla (D-61).
            ranking.Add(new JugadorRankingResponse
            {
                Posicion = i + 1,
                Nickname = jugadores[i].Nickname,
                Elo = jugadores[i].Elo,
            });
        }

        return ranking;
    }

    // Fuera de rango se ajusta en lugar de rechazarse, igual que la paginación (D-35).
    private static int AjustarCantidad(int cantidad)
    {
        if(cantidad > CantidadMaxima)
        {
            return CantidadMaxima;
        }

        if(cantidad < 1)
        {
            return CantidadPorDefecto;
        }

        return cantidad;
    }
}
