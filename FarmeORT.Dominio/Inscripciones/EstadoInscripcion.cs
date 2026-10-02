namespace FarmeORT.Dominio.Inscripciones;

public enum EstadoInscripcion
{
    /// <summary>
    /// El jugador participa del torneo. Es el estado con el que se crea la inscripción.
    /// </summary>
    Aceptada,

    /// <summary>
    /// El jugador se retiró antes de que empezara el torneo, o el torneo se canceló. Es final.
    /// </summary>
    Cancelada,

    /// <summary>
    /// El jugador quedó fuera al perder o abandonar una batalla. Es final.
    /// </summary>
    Eliminada,
}
