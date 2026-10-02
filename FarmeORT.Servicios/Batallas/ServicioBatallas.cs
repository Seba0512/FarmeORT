using FarmeORT.DTOs.Batallas;
using FarmeORT.IRepositorios.Batallas;
using FarmeORT.IRepositorios.Inscripciones;
using FarmeORT.IRepositorios.Torneos;
using FarmeORT.IServicios.Batallas;

namespace FarmeORT.Servicios.Batallas;

internal sealed class ServicioBatallas(
    IRepositorioBatallas repositorioBatallas,
    IRepositorioTorneos repositorioTorneos,
    IRepositorioInscripciones repositorioInscripciones) : IServicioBatallas
{
    public IReadOnlyList<BatallaResponse> Listar(FiltroBatallasRequest filtro, int usuarioId, string rol)
    {
        _ = repositorioBatallas;
        _ = repositorioTorneos;
        _ = repositorioInscripciones;
        throw new NotImplementedException();
    }

    public BatallaDetalleResponse ObtenerPorId(int id, int usuarioId, string rol)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<LanzamientoResponse> ListarLanzamientos(int id, int usuarioId, string rol)
    {
        throw new NotImplementedException();
    }
}
