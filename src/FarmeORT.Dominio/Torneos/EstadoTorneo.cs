namespace FarmeORT.Dominio.Torneos;

public enum EstadoTorneo
{
    /// <summary>
    /// Admite inscripciones. Es el estado con el que se crea el torneo.
    /// </summary>
    Abierto,

    /// <summary>
    /// Inscripciones cerradas: se disputan las batallas.
    /// </summary>
    EnCurso,

    /// <summary>
    /// Se conoce el campeón.
    /// </summary>
    Finalizado,

    /// <summary>
    /// No se disputará.
    /// </summary>
    Cancelado,
}
