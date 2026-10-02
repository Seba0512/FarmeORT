using FarmeORT.DTOs.Finalizaciones;
using FarmeORT.IRepositorios.Batallas;
using FarmeORT.IRepositorios.Inscripciones;
using FarmeORT.IRepositorios.Torneos;
using FarmeORT.IServicios.Finalizaciones;

namespace FarmeORT.Servicios.Finalizaciones;

internal sealed class ServicioFinalizaciones(
    IRepositorioTorneos repositorioTorneos,
    IRepositorioInscripciones repositorioInscripciones,
    IRepositorioBatallas repositorioBatallas) : IServicioFinalizaciones
{
    public FinalizacionResponse Finalizar(int torneoId, int usuarioId, string rol)
    {
        _ = repositorioTorneos;
        _ = repositorioInscripciones;
        _ = repositorioBatallas;
        _ = torneoId;
        _ = usuarioId;
        _ = rol;
        throw new NotImplementedException();
    }
}
