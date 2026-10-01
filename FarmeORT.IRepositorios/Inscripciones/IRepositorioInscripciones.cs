using FarmeORT.Dominio.Inscripciones;

namespace FarmeORT.IRepositorios.Inscripciones;

public interface IRepositorioInscripciones
{
    Inscripcion? ObtenerPorId(int id);

    IReadOnlyList<Inscripcion> Listar(FiltroInscripciones filtro);

    bool ExisteAceptada(int torneoId, int jugadorId);

    int ContarAceptadas(int torneoId);

    // Trae a cada jugador con su repertorio, para controlar las skills obligatorias.
    IReadOnlyList<Inscripcion> ListarAceptadasDelTorneo(int torneoId);

    void Agregar(Inscripcion inscripcion);

    void Actualizar(Inscripcion inscripcion);
}
