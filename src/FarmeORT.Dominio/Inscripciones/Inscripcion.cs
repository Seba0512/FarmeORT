using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.Excepciones;

namespace FarmeORT.Dominio.Inscripciones;

public sealed class Inscripcion
{
    public Inscripcion(Torneo torneo, Usuario jugador, DateTime fecha)
    {
        if(torneo.Estado != EstadoTorneo.Abierto)
        {
            throw new TorneoNoAbiertoException();
        }

        var faltantes = jugador.SkillsFaltantes(torneo.SkillsObligatorias);
        if(faltantes.Count > 0)
        {
            throw new SkillsObligatoriasFaltantesException(faltantes.Select(s => s.Codigo).ToList());
        }

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

    // Aceptada en un torneo que no está Finalizado ni Cancelado: el jugador todavía compite en él.
    public bool EstaVigente =>
        Estado == EstadoInscripcion.Aceptada && Torneo.Estado != EstadoTorneo.Finalizado && Torneo.Estado != EstadoTorneo.Cancelado;

    // No controla el estado del torneo: también se usa al cancelar un torneo En curso.
    public void Cancelar()
    {
        ValidarAceptada();

        Estado = EstadoInscripcion.Cancelada;
    }

    // Letra 1.4: solo una inscripción Aceptada en un torneo que no está Finalizado ni Cancelado exige las
    // obligatorias; una Eliminada ya no.
    public bool ExigeSkill(int skillId)
    {
        return EstaVigente && Torneo.SkillsObligatorias.Any(s => s.Id == skillId);
    }

    // La usa el fallo de una batalla: el perdedor queda fuera del torneo.
    public void Eliminar()
    {
        ValidarAceptada();

        Estado = EstadoInscripcion.Eliminada;
    }

    // Cancelada y Eliminada son estados finales.
    private void ValidarAceptada()
    {
        if(Estado != EstadoInscripcion.Aceptada)
        {
            throw new InscripcionNoActivaException();
        }
    }
}
