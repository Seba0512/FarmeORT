using FarmeORT.Dominio.Torneos;
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
        var organizador = repositorioUsuarios.ObtenerPorId(request.OrganizadorId.GetValueOrDefault())!;
        var torneo = new Torneo(
            request.Codigo,
            request.Nombre,
            request.Descripcion,
            request.FechaInicio.GetValueOrDefault(),
            request.FechaFin.GetValueOrDefault(),
            request.Cupo.GetValueOrDefault(),
            organizador,
            ObtenerFechaActual());
        repositorioTorneos.Agregar(torneo);
        return CrearResponse(torneo);
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

    private DateOnly ObtenerFechaActual()
    {
        return DateOnly.FromDateTime(reloj.GetLocalNow().DateTime);
    }

    private static TorneoResponse CrearResponse(Torneo torneo)
    {
        return new TorneoResponse
        {
            Id = torneo.Id,
            Codigo = torneo.Codigo,
            Nombre = torneo.Nombre,
            Descripcion = torneo.Descripcion,
            Estado = torneo.Estado.ToString(),
            FechaInicio = torneo.FechaInicio,
            FechaFin = torneo.FechaFin,
            Cupo = torneo.Cupo,
            OrganizadorId = torneo.OrganizadorId,
            Organizador = $"{torneo.Organizador.Nombre} {torneo.Organizador.Apellido}",
        };
    }
}
