using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;

namespace FarmeORT.Dominio.Batallas;

public sealed class Batalla
{
    public Batalla(Torneo torneo, int numeroRonda, Usuario jugador1, Usuario jugador2, ISorteo sorteo)
    {
        Torneo = torneo;
        TorneoId = torneo.Id;
        NumeroRonda = numeroRonda;
        Jugador1 = jugador1;
        Jugador1Id = jugador1.Id;
        Jugador2 = jugador2;
        Jugador2Id = jugador2.Id;
        Estado = EstadoBatalla.Pendiente;
        var jugadorInicial = jugador1.Elo.CompareTo(jugador2.Elo) switch
        {
            > 0 => jugador1,
            < 0 => jugador2,
            _ => sorteo.ElegirEntre(jugador1, jugador2),
        };
        JugadorInicialId = jugadorInicial.Id;
        JugadorTurnoId = JugadorInicialId;
    }

    public int Id { get; init; }

    public int TorneoId { get; private set; }

    public Torneo Torneo { get; private set; }

    public int NumeroRonda { get; private set; }

    public int Jugador1Id { get; private set; }

    public Usuario Jugador1 { get; private set; }

    public int Jugador2Id { get; private set; }

    public Usuario Jugador2 { get; private set; }

    public EstadoBatalla Estado { get; private set; }

    public int JugadorInicialId { get; private set; }

    public int? JugadorTurnoId { get; private set; }
}
