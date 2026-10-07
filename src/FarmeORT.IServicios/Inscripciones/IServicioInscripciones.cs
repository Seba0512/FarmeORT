using FarmeORT.DTOs.Inscripciones;

namespace FarmeORT.IServicios.Inscripciones;

public interface IServicioInscripciones
{
    InscripcionResponse Inscribir(int jugadorId, InscripcionRequest request);

    IReadOnlyList<InscripcionResponse> Listar(FiltroInscripcionesRequest filtro, int usuarioId, string rol);

    InscripcionResponse ObtenerPorId(int id, int usuarioId, string rol);

    InscripcionResponse Cancelar(int id, int jugadorId);
}
