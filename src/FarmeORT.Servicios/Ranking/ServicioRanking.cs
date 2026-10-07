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

    public IReadOnlyList<JugadorRankingResponse> Obtener(FiltroRankingRequest filtro, bool esAnonimo)
    {
        var jugadores = ListarJugadores(filtro.TorneoId, AjustarCantidad(filtro.Cantidad), esAnonimo);
        if(jugadores.Count == 0)
        {
            return [];
        }

        // Aunque se filtre por torneo, ganadas y perdidas son las globales de cada jugador (foro, debate 06).
        var resultados = repositorioBatallas.ContarResultadosPorJugador(jugadores.Select(j => j.Id).ToList());
        List<JugadorRankingResponse> ranking = [];
        for(var i = 0; i < jugadores.Count; i++)
        {
            // Un jugador sin batallas Finalizadas no figura en los resultados: tiene 0 ganadas y 0 perdidas.
            var ganadas = 0;
            var perdidas = 0;
            if(resultados.TryGetValue(jugadores[i].Id, out var resultado))
            {
                ganadas = resultado.Ganadas;
                perdidas = resultado.Perdidas;
            }

            ranking.Add(new JugadorRankingResponse
            {
                Posicion = i + 1,
                Nickname = jugadores[i].Nickname,
                Elo = jugadores[i].Elo,
                BatallasGanadas = ganadas,
                BatallasPerdidas = perdidas,
            });
        }

        return ranking;
    }

    private IReadOnlyList<Usuario> ListarJugadores(int? torneoId, int cantidad, bool esAnonimo)
    {
        if(torneoId == null)
        {
            return repositorioUsuarios.ListarRanking(cantidad);
        }

        ValidarTorneoVisible(torneoId.Value, esAnonimo);
        return repositorioUsuarios.ListarRankingDelTorneo(torneoId.Value, cantidad);
    }

    // Para el anónimo un torneo Cancelado no existe; el autenticado lo ve, como en la consulta de torneos (D-36,
    // foro, debate 11).
    private void ValidarTorneoVisible(int torneoId, bool esAnonimo)
    {
        var torneo = repositorioTorneos.ObtenerPorId(torneoId);
        if(torneo == null)
        {
            throw new TorneoNoEncontradoException();
        }

        if(esAnonimo && !torneo.EsVisibleParaAnonimos)
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
            return FiltroRankingRequest.CantidadPorDefecto;
        }

        return cantidad;
    }
}
