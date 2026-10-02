using FarmeORT.Dominio.Batallas;
using FarmeORT.DTOs.Batallas;

namespace FarmeORT.Servicios.Batallas;

internal static class RespuestasBatallas
{
    public static BatallaResponse Resumen(Batalla batalla)
    {
        return new BatallaResponse
        {
            Id = batalla.Id,
            TorneoId = batalla.TorneoId,
            Ronda = batalla.NumeroRonda,
            Jugador1 = new JugadorBatallaResponse { Id = batalla.Jugador1Id, Nickname = batalla.Jugador1.Nickname, Elo = batalla.Jugador1.Elo },
            Jugador2 = new JugadorBatallaResponse { Id = batalla.Jugador2Id, Nickname = batalla.Jugador2.Nickname, Elo = batalla.Jugador2.Elo },
            Estado = batalla.Estado.ToString(),
            JugadorTurnoId = batalla.JugadorTurnoId,
            GanadorId = batalla.GanadorId,
        };
    }

    public static BatallaDetalleResponse Detalle(Batalla batalla)
    {
        return new BatallaDetalleResponse
        {
            Id = batalla.Id,
            TorneoId = batalla.TorneoId,
            Ronda = batalla.NumeroRonda,
            Jugador1 = new JugadorBatallaResponse { Id = batalla.Jugador1Id, Nickname = batalla.Jugador1.Nickname, Elo = batalla.Jugador1.Elo },
            Jugador2 = new JugadorBatallaResponse { Id = batalla.Jugador2Id, Nickname = batalla.Jugador2.Nickname, Elo = batalla.Jugador2.Elo },
            Estado = batalla.Estado.ToString(),
            JugadorTurnoId = batalla.JugadorTurnoId,
            GanadorId = batalla.GanadorId,
            Comentario = batalla.Comentario,
            CambioEloJugador1 = batalla.CambioEloJugador1,
            CambioEloJugador2 = batalla.CambioEloJugador2,
            Lanzamientos = batalla.Lanzamientos.OrderBy(l => l.Turno).Select(l => new LanzamientoResponse
            {
                Turno = l.Turno,
                JugadorId = l.JugadorId,
                Nickname = l.Jugador.Nickname,
                SkillId = l.SkillId,
                Skill = l.Skill.Nombre,
                Fecha = l.Fecha,
            }).ToList(),
        };
    }
}
