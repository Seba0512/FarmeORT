using FarmeORT.DTOs.Inscripciones;

namespace FarmeORT.IServicios.Inscripciones;

public interface IServicioInscripciones
{
    InscripcionResponse Inscribir(int jugadorId, InscripcionRequest request);
}
