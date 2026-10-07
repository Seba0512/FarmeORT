using FarmeORT.DTOs.Batallas;

namespace FarmeORT.IServicios.Batallas;

public interface IServicioFallos
{
    BatallaDetalleResponse Fallar(int batallaId, int organizadorId, FalloRequest request);
}
