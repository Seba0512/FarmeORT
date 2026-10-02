using FarmeORT.Dominio.Batallas;
using FarmeORT.Dominio.Rondas;
using FarmeORT.DTOs.Rondas;
using FarmeORT.IRepositorios.Batallas;
using FarmeORT.IRepositorios.Inscripciones;
using FarmeORT.IRepositorios.Torneos;
using FarmeORT.IServicios.Rondas;

namespace FarmeORT.Servicios.Rondas;

internal sealed class ServicioRondas(
    IRepositorioTorneos repositorioTorneos,
    IRepositorioInscripciones repositorioInscripciones,
    IRepositorioBatallas repositorioBatallas,
    IEstrategiaEmparejamiento estrategiaEmparejamiento,
    ISorteo sorteo) : IServicioRondas
{
    public RondaResponse Generar(int torneoId, int usuarioId, string rol)
    {
        _ = repositorioTorneos;
        _ = repositorioInscripciones;
        _ = repositorioBatallas;
        _ = estrategiaEmparejamiento;
        _ = sorteo;
        _ = torneoId;
        _ = usuarioId;
        _ = rol;
        throw new NotImplementedException();
    }
}
