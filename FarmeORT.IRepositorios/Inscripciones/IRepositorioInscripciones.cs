using FarmeORT.Dominio.Inscripciones;

namespace FarmeORT.IRepositorios.Inscripciones;

public interface IRepositorioInscripciones
{
    Inscripcion? ObtenerPorId(int id);

    IReadOnlyList<Inscripcion> Listar(FiltroInscripciones filtro);

    bool ExisteAceptada(int torneoId, int jugadorId);

    int ContarAceptadas(int torneoId);

    // Una sola consulta para toda la página del listado de torneos; un torneo sin aceptadas no figura.
    IReadOnlyDictionary<int, int> ContarAceptadasPorTorneo(IReadOnlyCollection<int> torneoIds);

    // Trae a cada jugador con su repertorio, para controlar las skills obligatorias.
    IReadOnlyList<Inscripcion> ListarAceptadasDelTorneo(int torneoId);

    // Trae cada torneo con sus skills obligatorias, para controlar si se puede quitar una skill del repertorio.
    IReadOnlyList<Inscripcion> ListarAceptadasDelJugador(int jugadorId);

    void Agregar(Inscripcion inscripcion);

    void Actualizar(Inscripcion inscripcion);

    // Persona B: inscripción rastreada del perdedor para el único guardado del fallo.
    Inscripcion? ObtenerAceptada(int torneoId, int jugadorId);

    // Persona B: la inscripción histórica permite consultar batallas aun después de ser eliminado.
    bool ExisteDelJugador(int torneoId, int jugadorId);
}
