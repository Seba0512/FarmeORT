using FarmeORT.DTOs.Ranking;
using FarmeORT.IRepositorios.Usuarios;
using FarmeORT.IServicios.Ranking;

namespace FarmeORT.Servicios.Ranking;

internal sealed class ServicioRanking(IRepositorioUsuarios repositorioUsuarios) : IServicioRanking
{
    public IReadOnlyList<JugadorRankingResponse> Obtener(FiltroRankingRequest filtro)
    {
        _ = repositorioUsuarios;
        _ = filtro;
        throw new NotImplementedException();
    }
}
