using FarmeORT.DTOs.Ranking;

namespace FarmeORT.IServicios.Ranking;

public interface IServicioRanking
{
    IReadOnlyList<JugadorRankingResponse> Obtener(FiltroRankingRequest filtro, bool esAnonimo);
}
