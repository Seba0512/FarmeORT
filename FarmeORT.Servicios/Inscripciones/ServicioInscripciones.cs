using FarmeORT.DTOs.Inscripciones;
using FarmeORT.IRepositorios.Inscripciones;
using FarmeORT.IRepositorios.Torneos;
using FarmeORT.IRepositorios.Usuarios;
using FarmeORT.IServicios.Inscripciones;

namespace FarmeORT.Servicios.Inscripciones;

internal sealed class ServicioInscripciones(
    IRepositorioInscripciones repositorioInscripciones,
    IRepositorioTorneos repositorioTorneos,
    IRepositorioUsuarios repositorioUsuarios,
    TimeProvider reloj) : IServicioInscripciones
{
    public InscripcionResponse Inscribir(int jugadorId, InscripcionRequest request)
    {
        _ = repositorioInscripciones;
        _ = repositorioTorneos;
        _ = repositorioUsuarios;
        _ = reloj;
        _ = jugadorId;
        _ = request;
        throw new NotImplementedException();
    }

    public IReadOnlyList<InscripcionResponse> Listar(FiltroInscripcionesRequest filtro, int usuarioId, string rol)
    {
        _ = filtro;
        _ = usuarioId;
        _ = rol;
        throw new NotImplementedException();
    }

    public InscripcionResponse ObtenerPorId(int id, int usuarioId, string rol)
    {
        _ = id;
        _ = usuarioId;
        _ = rol;
        throw new NotImplementedException();
    }

    public InscripcionResponse Cancelar(int id, int jugadorId)
    {
        _ = id;
        _ = jugadorId;
        throw new NotImplementedException();
    }
}
