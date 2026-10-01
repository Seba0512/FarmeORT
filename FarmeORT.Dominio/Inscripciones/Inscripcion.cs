using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;

namespace FarmeORT.Dominio.Inscripciones;

public sealed class Inscripcion
{
    public Inscripcion(Torneo torneo, Usuario jugador, DateTime fecha)
    {
        Torneo = torneo;
        TorneoId = torneo.Id;
        Jugador = jugador;
        JugadorId = jugador.Id;
        Fecha = fecha;
        Estado = EstadoInscripcion.Aceptada;
    }

    // EF Core materializa la inscripción con este constructor y completa las navegaciones después (D-08).
    private Inscripcion()
    {
        Torneo = null!;
        Jugador = null!;
    }

    public int Id { get; init; }

    public int TorneoId { get; private set; }

    public Torneo Torneo { get; private set; }

    public int JugadorId { get; private set; }

    public Usuario Jugador { get; private set; }

    public EstadoInscripcion Estado { get; private set; }

    public DateTime Fecha { get; private set; }
}
