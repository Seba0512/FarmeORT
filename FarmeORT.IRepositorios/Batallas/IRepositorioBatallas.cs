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

    // Filtra y pagina las batallas de un torneo; el servicio ya controló que quien consulta puede verlo (D-55).
    IReadOnlyList<Batalla> Listar(FiltroBatallas filtro);

    // Ganadas y perdidas de los jugadores del ranking en una sola consulta, opcionalmente de un torneo (D-46).
    // Un jugador sin batallas Finalizadas no figura.
    IReadOnlyDictionary<int, ResultadosDeBatallas> ContarResultadosPorJugador(IReadOnlyCollection<int> jugadorIds, int? torneoId);

    // Las batallas del torneo con sus jugadores, ordenadas por ronda y por id, para armar las rondas (D-62).
    IReadOnlyList<Batalla> ListarDelTorneo(int torneoId);

    // Guarda las batallas nuevas de la ronda y el estado del torneo en un solo SaveChanges (D-62).
    void GuardarRonda(Torneo torneo, IReadOnlyList<Batalla> batallas);
}
