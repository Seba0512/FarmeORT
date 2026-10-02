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
        if(jugador1.Id == jugador2.Id)
        {
            throw new DatosInvalidosException("La batalla requiere dos jugadores distintos.");
        }

        if(jugador1.Rol != Rol.Jugador || jugador2.Rol != Rol.Jugador)
        {
            throw new DatosInvalidosException("Los participantes de la batalla deben tener rol Jugador.");
        }

        if(numeroRonda < 1)
        {
            throw new DatosInvalidosException("El número de ronda debe ser positivo.");
        }

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

    // EF Core materializa las navegaciones después, como en Torneo (D-08).
    private Batalla()
    {
        Torneo = null!;
        Jugador1 = null!;
        Jugador2 = null!;
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

    public int? GanadorId { get; private set; }

    public Usuario? Ganador { get; private set; }

    public string? Comentario { get; private set; }

    public int? CambioEloJugador1 { get; private set; }

    public int? CambioEloJugador2 { get; private set; }

    public void Fallar(int ganadorId, string? comentario, int cambioEloJugador1, int cambioEloJugador2)
    {
        throw new NotImplementedException();
    }

    public Lanzamiento Lanzar(Usuario jugador, Skill skill, DateTimeOffset fecha)
    {
        ValidarLanzamiento(jugador, skill);

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

    private void ValidarLanzamiento(Usuario jugador, Skill skill)
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

        if(!jugador.Repertorio.Any(d => d.SkillId == skill.Id))
        {
            throw new SkillFueraDelRepertorioException();
        }

        if(_lanzamientos.Any(l => l.JugadorId == jugador.Id && l.SkillId == skill.Id))
        {
            throw new SkillYaLanzadaException();
        }

        var propios = _lanzamientos.Where(l => l.JugadorId == jugador.Id).ToList();
        var pendientes = Torneo.SkillsObligatorias
            .Where(obligatoria => propios.All(l => l.SkillId != obligatoria.Id)).ToList();
        var turnosRestantes = (LanzamientosTotales / 2) - propios.Count;
        if(pendientes.Count == turnosRestantes && pendientes.All(obligatoria => obligatoria.Id != skill.Id))
        {
            throw new SkillObligatoriaPendienteException();
        }
    }
}
