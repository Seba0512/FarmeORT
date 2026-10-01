using FarmeORT.DTOs.Batallas;
using FarmeORT.IRepositorios.Batallas;
using FarmeORT.IServicios.Batallas;

namespace FarmeORT.Servicios.Batallas;

internal sealed class ServicioLanzamientos(IRepositorioBatallas repositorioBatallas) : IServicioLanzamientos
{
    public LanzamientoResultadoResponse Lanzar(int batallaId, int jugadorId, LanzamientoRequest request)
    {
        throw new NotImplementedException(repositorioBatallas.GetType().Name);
    }
}
