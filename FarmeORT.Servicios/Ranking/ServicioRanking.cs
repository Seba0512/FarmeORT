using FarmeORT.DTOs.Ranking;
using FarmeORT.IRepositorios.Usuarios;
using FarmeORT.IServicios.Ranking;

namespace FarmeORT.Servicios.Ranking;

internal sealed class ServicioRanking(IRepositorioUsuarios repositorioUsuarios) : IServicioRanking
{
    private const int CantidadMaxima = 100;

    public IReadOnlyList<JugadorRankingResponse> Obtener(FiltroRankingRequest filtro)
    {
        var cantidad = filtro.Cantidad;
        if(cantidad > CantidadMaxima)
        {
            cantidad = CantidadMaxima;
        }

        var jugadores = repositorioUsuarios.ListarRanking(cantidad);
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
}
