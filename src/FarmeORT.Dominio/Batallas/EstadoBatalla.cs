namespace FarmeORT.Dominio.Batallas;

public enum EstadoBatalla
{
    /// <summary>
    /// Admite lanzamientos alternados de los jugadores.
    /// </summary>
    Pendiente,

    /// <summary>
    /// Los ocho lanzamientos se completaron y falta el fallo.
    /// </summary>
    EnArbitraje,

    /// <summary>
    /// El organizador ya resolvió la batalla.
    /// </summary>
    Finalizada,

    /// <summary>
    /// La batalla no se completará y no admite lanzamientos.
    /// </summary>
    Cancelada,
}
