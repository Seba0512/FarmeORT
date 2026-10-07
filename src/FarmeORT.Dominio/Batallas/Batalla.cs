using FarmeORT.Dominio.Skills;
using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.Excepciones;

namespace FarmeORT.Dominio.Batallas;

public sealed class Batalla
{
    private const int LanzamientosTotales = 8;
    private const int LanzamientosPorJugador = 4;
    private const int LargoMinimoComentario = 10;
    private const int LargoMaximoComentario = 500;

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
        JugadorInicialId = ElegirJugadorInicial(jugador1, jugador2, sorteo).Id;
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
        ValidarFallo(ganadorId, comentario);

        Ganador = ganadorId == Jugador1Id ? Jugador1 : Jugador2;
        GanadorId = ganadorId;
        Comentario = comentario;
        CambioEloJugador1 = cambioEloJugador1;
        CambioEloJugador2 = cambioEloJugador2;
        Estado = EstadoBatalla.Finalizada;
        JugadorTurnoId = null;
    }

    public void ValidarFallo(int ganadorId, string? comentario)
    {
        if(Estado != EstadoBatalla.Pendiente && Estado != EstadoBatalla.EnArbitraje)
        {
            throw new BatallaNoFallableException();
        }

        if(ganadorId != Jugador1Id && ganadorId != Jugador2Id)
        {
            throw new JugadorNoParticipaEnBatallaException();
        }

        if(string.IsNullOrWhiteSpace(comentario)
            || comentario.Length < LargoMinimoComentario
            || comentario.Length > LargoMaximoComentario)
        {
            throw new DatosInvalidosException("El comentario debe tener entre 10 y 500 caracteres y no puede estar vacío.");
        }
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

        // Si le quedan tantas obligatorias pendientes como turnos, solo puede lanzar una obligatoria (letra 1.8).
        var pendientes = ObligatoriasPendientes(jugador);
        var turnosRestantes = LanzamientosPorJugador - CantidadDeLanzamientos(jugador);
        var lanzaUnaPendiente = pendientes.Any(obligatoria => obligatoria.Id == skill.Id);
        if(pendientes.Count == turnosRestantes && !lanzaUnaPendiente)
        {
            throw new SkillObligatoriaPendienteException();
        }
    }

    // Empieza el de mayor ELO; si empatan, decide el sorteo.
    private static Usuario ElegirJugadorInicial(Usuario jugador1, Usuario jugador2, ISorteo sorteo)
    {
        if(jugador1.Elo > jugador2.Elo)
        {
            return jugador1;
        }

        if(jugador2.Elo > jugador1.Elo)
        {
            return jugador2;
        }

        return sorteo.ElegirEntre(jugador1, jugador2);
    }

    // Las obligatorias del torneo que el jugador todavía no lanzó en esta batalla.
    private List<Skill> ObligatoriasPendientes(Usuario jugador)
    {
        List<Skill> pendientes = [];
        foreach(var obligatoria in Torneo.SkillsObligatorias)
        {
            var yaLaLanzo = _lanzamientos.Any(l => l.JugadorId == jugador.Id && l.SkillId == obligatoria.Id);
            if(!yaLaLanzo)
            {
                pendientes.Add(obligatoria);
            }
        }

        return pendientes;
    }

    private int CantidadDeLanzamientos(Usuario jugador)
    {
        return _lanzamientos.Count(l => l.JugadorId == jugador.Id);
    }
}
