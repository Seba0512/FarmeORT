using FarmeORT.Dominio.Skills;
using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.Excepciones;

namespace FarmeORT.Dominio.Batallas;

public sealed class Batalla
{
    private const int LanzamientosTotales = 8;

    private readonly List<Lanzamiento> _lanzamientos = [];

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

    public IReadOnlyCollection<Lanzamiento> Lanzamientos => _lanzamientos.AsReadOnly();

    public Lanzamiento Lanzar(Usuario jugador, Skill skill, DateTimeOffset fecha)
    {
        if(Estado != EstadoBatalla.Pendiente)
        {
            throw new BatallaNoPendienteException();
        }

        if(jugador.Id != Jugador1Id && jugador.Id != Jugador2Id)
        {
            throw new JugadorNoParticipaEnBatallaException();
        }

        if(jugador.Id != JugadorTurnoId)
        {
            throw new TurnoIncorrectoException();
        }

        var lanzamiento = new Lanzamiento(this, _lanzamientos.Count + 1, jugador, skill, fecha);
        _lanzamientos.Add(lanzamiento);
        JugadorTurnoId = jugador.Id == Jugador1Id ? Jugador2Id : Jugador1Id;
        if(_lanzamientos.Count == LanzamientosTotales)
        {
            Estado = EstadoBatalla.EnArbitraje;
            JugadorTurnoId = null;
        }

        return lanzamiento;
    }

    public void Cancelar()
    {
        if(Estado != EstadoBatalla.Pendiente && Estado != EstadoBatalla.EnArbitraje)
        {
            throw new BatallaNoCancelableException();
        }

        Estado = EstadoBatalla.Cancelada;
        JugadorTurnoId = null;
    }
}
