using FarmeORT.DTOs.Batallas;

namespace FarmeORT.IServicios.Batallas;

public interface IServicioBatallas
{
    IReadOnlyList<BatallaResponse> Listar(FiltroBatallasRequest filtro, int usuarioId, string rol);

    BatallaDetalleResponse ObtenerPorId(int id, int usuarioId, string rol);

    IReadOnlyList<LanzamientoResponse> ListarLanzamientos(int id, int usuarioId, string rol);
}
