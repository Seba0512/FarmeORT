using FarmeORT.Dominio.Elo;
using FarmeORT.DTOs.Batallas;
using FarmeORT.IRepositorios.Batallas;
using FarmeORT.IRepositorios.Inscripciones;
using FarmeORT.IServicios.Batallas;

namespace FarmeORT.Servicios.Batallas;

internal sealed class ServicioFallos(
    IRepositorioBatallas repositorioBatallas,
    IRepositorioInscripciones repositorioInscripciones,
    ICalculadorElo calculadorElo) : IServicioFallos
{
    public BatallaDetalleResponse Fallar(int batallaId, int organizadorId, FalloRequest request)
    {
        _ = repositorioBatallas;
        _ = repositorioInscripciones;
        _ = calculadorElo;
        throw new NotImplementedException();
    }
}
