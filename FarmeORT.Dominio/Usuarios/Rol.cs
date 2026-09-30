namespace FarmeORT.Dominio.Usuarios;

public enum Rol
{
    /// <summary>
    /// Participa en torneos: declara su repertorio, se inscribe y lanza skills en batalla.
    /// </summary>
    Jugador,

    /// <summary>
    /// Gestiona los torneos que tiene asignados: skills obligatorias, rondas y fallos de batallas.
    /// </summary>
    Organizador,

    /// <summary>
    /// Administra usuarios, el catálogo de skills y los torneos.
    /// </summary>
    Administrador,
}
