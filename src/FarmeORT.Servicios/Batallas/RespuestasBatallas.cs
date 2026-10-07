using FarmeORT.Dominio.Batallas;
using FarmeORT.Dominio.Usuarios;
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
            Jugador1 = Jugador(batalla.Jugador1),
            Jugador2 = Jugador(batalla.Jugador2),
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
            Jugador1 = Jugador(batalla.Jugador1),
            Jugador2 = Jugador(batalla.Jugador2),
            Estado = batalla.Estado.ToString(),
            JugadorTurnoId = batalla.JugadorTurnoId,
            GanadorId = batalla.GanadorId,
            Comentario = batalla.Comentario,
            CambioEloJugador1 = batalla.CambioEloJugador1,
            CambioEloJugador2 = batalla.CambioEloJugador2,
            Lanzamientos = Lanzamientos(batalla),
        };
    }

    public static IReadOnlyList<LanzamientoResponse> Lanzamientos(Batalla batalla)
    {
        return batalla.Lanzamientos.OrderBy(l => l.Turno).Select(l => new LanzamientoResponse
        {
            Turno = l.Turno,
            JugadorId = l.JugadorId,
            Nickname = l.Jugador.Nickname,
            SkillId = l.SkillId,
            Skill = l.Skill.Nombre,
            Fecha = l.Fecha,
        }).ToList();
    }

    private static JugadorBatallaResponse Jugador(Usuario jugador)
    {
        return new JugadorBatallaResponse { Id = jugador.Id, Nickname = jugador.Nickname, Elo = jugador.Elo };
    }
}
