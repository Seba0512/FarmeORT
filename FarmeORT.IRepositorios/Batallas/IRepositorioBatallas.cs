using FarmeORT.Dominio.Batallas;

namespace FarmeORT.IRepositorios.Batallas;

public interface IRepositorioBatallas
{
    Batalla? ObtenerPorId(int id);

    void Guardar();

    // Batallas Pendientes o EnArbitraje del torneo, rastreadas para cancelarlas en el mismo guardado que el torneo.
    IReadOnlyList<Batalla> ListarSinResolverDelTorneo(int torneoId);

    // Batallas Pendientes o EnArbitraje en las que juega: impiden dar de baja al usuario o cambiarle el rol.
    bool ExisteSinResolverDelJugador(int jugadorId);

    // Persona B: filtros y paginación de batallas de un torneo ya autorizado.
    IReadOnlyList<Batalla> Listar(FiltroBatallas filtro);

    // Persona C: deja la batalla nueva rastreada para guardarla junto con el torneo (D-62).
    void Agregar(Batalla batalla);

    // Persona C: las batallas del torneo con sus jugadores, ordenadas por ronda y por id, para armar las rondas.
    IReadOnlyList<Batalla> ListarDelTorneo(int torneoId);
}
