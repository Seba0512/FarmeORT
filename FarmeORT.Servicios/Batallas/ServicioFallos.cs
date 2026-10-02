using FarmeORT.Dominio.Elo;
using FarmeORT.DTOs.Batallas;
using FarmeORT.Excepciones;
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
        var ganadorId = DatosObligatorios.Obtener(request.GanadorId, "El ganador es obligatorio.");
        if(ganadorId < 1)
        {
            throw new DatosInvalidosException("El id del ganador debe ser positivo.");
        }

        var batalla = repositorioBatallas.ObtenerPorId(batallaId);
        if(batalla == null)
        {
            throw new BatallaNoEncontradaException();
        }

        if(batalla.Torneo.OrganizadorId != organizadorId)
        {
            throw new NoEsOrganizadorDelTorneoException();
        }

        _ = repositorioInscripciones;
        _ = calculadorElo;
        throw new NotImplementedException();
    }
}
