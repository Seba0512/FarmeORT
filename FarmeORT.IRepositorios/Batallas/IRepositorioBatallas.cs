using FarmeORT.Dominio.Batallas;
using FarmeORT.Dominio.Torneos;

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

    // Persona A: ganadas y perdidas de los jugadores del ranking en una sola consulta, opcionalmente de un torneo.
    // Un jugador sin batallas Finalizadas no figura.
    IReadOnlyDictionary<int, ResultadosDeBatallas> ContarResultadosPorJugador(IReadOnlyCollection<int> jugadorIds, int? torneoId);

    // Persona C: deja la batalla nueva rastreada para guardarla junto con el torneo (D-62).
    void Agregar(Batalla batalla);

    // Persona C: las batallas del torneo con sus jugadores, ordenadas por ronda y por id, para armar las rondas.
    IReadOnlyList<Batalla> ListarDelTorneo(int torneoId);

    // Persona C: guarda las batallas nuevas de la ronda y el estado del torneo en un solo SaveChanges (D-62).
    void GuardarRonda(Torneo torneo, IReadOnlyList<Batalla> batallas);
}
