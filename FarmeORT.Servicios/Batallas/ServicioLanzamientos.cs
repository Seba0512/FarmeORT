using FarmeORT.DTOs.Batallas;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Batallas;
using FarmeORT.IServicios.Batallas;

namespace FarmeORT.Servicios.Batallas;

internal sealed class ServicioLanzamientos(IRepositorioBatallas repositorioBatallas) : IServicioLanzamientos
{
    public LanzamientoResultadoResponse Lanzar(int batallaId, int jugadorId, LanzamientoRequest request)
    {
        var batalla = repositorioBatallas.ObtenerPorId(batallaId);
        if(batalla == null)
        {
            throw new BatallaNoEncontradaException();
        }

        throw new NotImplementedException();
    }
}
