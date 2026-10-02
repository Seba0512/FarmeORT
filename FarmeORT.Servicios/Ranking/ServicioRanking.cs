using FarmeORT.Dominio.Usuarios;
using FarmeORT.DTOs.Ranking;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Batallas;
using FarmeORT.IRepositorios.Torneos;
using FarmeORT.IRepositorios.Usuarios;
using FarmeORT.IServicios.Ranking;

namespace FarmeORT.Servicios.Ranking;

internal sealed class ServicioRanking(
    IRepositorioUsuarios repositorioUsuarios,
    IRepositorioTorneos repositorioTorneos,
    IRepositorioBatallas repositorioBatallas) : IServicioRanking
{
    private const int CantidadMaxima = 100;
    private const int CantidadPorDefecto = 10;

    public IReadOnlyList<JugadorRankingResponse> Obtener(FiltroRankingRequest filtro)
    {
        var jugadores = ListarJugadores(filtro.TorneoId, AjustarCantidad(filtro.Cantidad));
        if(jugadores.Count == 0)
        {
            return [];
        }

        var resultados = repositorioBatallas.ContarResultadosPorJugador(jugadores.Select(j => j.Id).ToList(), filtro.TorneoId);
        List<JugadorRankingResponse> ranking = [];
        for(var i = 0; i < jugadores.Count; i++)
        {
            var resultado = resultados.GetValueOrDefault(jugadores[i].Id);
            ranking.Add(new JugadorRankingResponse
            {
                Posicion = i + 1,
                Nickname = jugadores[i].Nickname,
                Elo = jugadores[i].Elo,
                BatallasGanadas = resultado?.Ganadas ?? 0,
                BatallasPerdidas = resultado?.Perdidas ?? 0,
            });
        }

        return ranking;
    }

    private IReadOnlyList<Usuario> ListarJugadores(int? torneoId, int cantidad)
    {
        if(torneoId == null)
        {
            return repositorioUsuarios.ListarRanking(cantidad);
        }

        ValidarTorneoVisible(torneoId.Value);
        return repositorioUsuarios.ListarRankingDelTorneo(torneoId.Value, cantidad);
    }

    // El ranking es público y para el público un torneo cancelado no existe (D-36).
    private void ValidarTorneoVisible(int torneoId)
    {
        var torneo = repositorioTorneos.ObtenerPorId(torneoId);
        if(torneo == null || !torneo.EsVisibleParaAnonimos)
        {
            throw new TorneoNoEncontradoException();
        }
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
