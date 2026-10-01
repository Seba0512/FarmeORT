using FarmeORT.Dominio.Inscripciones;

namespace FarmeORT.IRepositorios.Inscripciones;

public interface IRepositorioInscripciones
{
    bool ExisteAceptada(int torneoId, int jugadorId);

    void Agregar(Inscripcion inscripcion);
}
