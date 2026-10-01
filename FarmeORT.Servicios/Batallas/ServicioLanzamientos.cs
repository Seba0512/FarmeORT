using FarmeORT.DTOs.Batallas;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Batallas;
using FarmeORT.IRepositorios.Skills;
using FarmeORT.IRepositorios.Usuarios;
using FarmeORT.IServicios.Batallas;

namespace FarmeORT.Servicios.Batallas;

internal sealed class ServicioLanzamientos(
    IRepositorioBatallas repositorioBatallas,
    IRepositorioUsuarios repositorioUsuarios,
    IRepositorioSkills repositorioSkills,
    TimeProvider reloj) : IServicioLanzamientos
{
    public LanzamientoResultadoResponse Lanzar(int batallaId, int jugadorId, LanzamientoRequest request)
    {
        var batalla = repositorioBatallas.ObtenerPorId(batallaId);
        if(batalla == null)
        {
            throw new BatallaNoEncontradaException();
        }

        throw new NotImplementedException($"{repositorioUsuarios.GetType().Name} {repositorioSkills.GetType().Name} {reloj.GetType().Name}");
    }
}
