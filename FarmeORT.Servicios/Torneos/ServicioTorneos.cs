using FarmeORT.DTOs.Torneos;
using FarmeORT.IRepositorios.Torneos;
using FarmeORT.IRepositorios.Usuarios;
using FarmeORT.IServicios.Torneos;

namespace FarmeORT.Servicios.Torneos;

internal sealed class ServicioTorneos(
    IRepositorioTorneos repositorioTorneos,
    IRepositorioUsuarios repositorioUsuarios,
    TimeProvider reloj) : IServicioTorneos
{
    public TorneoResponse Crear(TorneoRequest request)
    {
        _ = repositorioTorneos;
        _ = repositorioUsuarios;
        _ = reloj;
        _ = request;
        throw new NotImplementedException();
    }

    public TorneoDetalleResponse ObtenerPorId(int id, bool esAnonimo)
    {
        _ = id;
        _ = esAnonimo;
        throw new NotImplementedException();
    }

    public IReadOnlyList<TorneoResponse> Listar(FiltroTorneosRequest filtro, bool esAnonimo)
    {
        _ = filtro;
        _ = esAnonimo;
        throw new NotImplementedException();
    }

    public TorneoResponse Modificar(int id, TorneoRequest request)
    {
        _ = id;
        _ = request;
        throw new NotImplementedException();
    }

    public TorneoResponse Cancelar(int id)
    {
        _ = id;
        throw new NotImplementedException();
    }
}
