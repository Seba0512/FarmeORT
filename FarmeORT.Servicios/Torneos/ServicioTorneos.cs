using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.DTOs.Torneos;
using FarmeORT.Excepciones;
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
        var fechaInicio = ObtenerObligatorio(request.FechaInicio, "La fecha de inicio es obligatoria.");
        var fechaFin = ObtenerObligatorio(request.FechaFin, "La fecha de fin es obligatoria.");
        var cupo = ObtenerObligatorio(request.Cupo, "El cupo es obligatorio.");
        var organizadorId = ObtenerObligatorio(request.OrganizadorId, "El organizador es obligatorio.");
        ValidarCodigoDisponible(request.Codigo);
        var organizador = ObtenerUsuarioExistente(organizadorId);
        var torneo = new Torneo(
            request.Codigo,
            request.Nombre,
            request.Descripcion,
            fechaInicio,
            fechaFin,
            cupo,
            organizador,
            ObtenerFechaActual());
        repositorioTorneos.Agregar(torneo);
        return CrearResponse<TorneoResponse>(torneo);
    }

    public TorneoDetalleResponse ObtenerPorId(int id, bool esAnonimo)
    {
        _ = esAnonimo;
        var torneo = ObtenerTorneoExistente(id);
        return CrearResponse<TorneoDetalleResponse>(torneo);
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

    private Torneo ObtenerTorneoExistente(int id)
    {
        var torneo = repositorioTorneos.ObtenerPorId(id);
        if(torneo == null)
        {
            throw new TorneoNoEncontradoException();
        }

        return torneo;
    }

    private Usuario ObtenerUsuarioExistente(int id)
    {
        var usuario = repositorioUsuarios.ObtenerPorId(id);
        if(usuario == null)
        {
            throw new UsuarioNoEncontradoException();
        }

        return usuario;
    }

    private void ValidarCodigoDisponible(string codigo)
    {
        if(repositorioTorneos.ExistePorCodigo(codigo))
        {
            throw new CodigoTorneoYaExisteException();
        }
    }

    // El DTO ya exige estos datos; el servicio los vuelve a controlar porque no depende de quién lo invoque.
    private static T ObtenerObligatorio<T>(T? valor, string mensaje)
        where T : struct
    {
        if(valor == null)
        {
            throw new DatosInvalidosException(mensaje);
        }

        return valor.Value;
    }

    private DateOnly ObtenerFechaActual()
    {
        return DateOnly.FromDateTime(reloj.GetLocalNow().DateTime);
    }

    // El detalle repite los datos del resumen: el mismo mapeo sirve para los dos tipos de respuesta.
    private static T CrearResponse<T>(Torneo torneo)
        where T : TorneoResponse, new()
    {
        return new T
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
