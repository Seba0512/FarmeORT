using FarmeORT.DTOs.Batallas;

namespace FarmeORT.IServicios.Batallas;

public interface IServicioLanzamientos
{
    LanzamientoResultadoResponse Lanzar(int batallaId, int jugadorId, LanzamientoRequest request);
}
