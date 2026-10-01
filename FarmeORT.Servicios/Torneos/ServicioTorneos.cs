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
        var datos = ObtenerDatosDelTorneo(request);
        ValidarCodigoDisponible(request.Codigo);
        var organizador = ObtenerUsuarioExistente(datos.OrganizadorId);
        var torneo = new Torneo(
            request.Codigo,
            request.Nombre,
            request.Descripcion,
            datos.FechaInicio,
            datos.FechaFin,
            datos.Cupo,
            organizador,
            ObtenerFechaActual());
        repositorioTorneos.Agregar(torneo);
        return CrearResponse<TorneoResponse>(torneo);
    }

    public TorneoDetalleResponse ObtenerPorId(int id, bool esAnonimo)
    {
        var torneo = ObtenerTorneoExistente(id);

        // Para el anónimo un torneo cancelado no existe: se responde igual que si no estuviera (D-36).
        if(esAnonimo && torneo.Estado == EstadoTorneo.Cancelado)
        {
            throw new TorneoNoEncontradoException();
        }

        return CrearResponse<TorneoDetalleResponse>(torneo);
    }

    public IReadOnlyList<TorneoResponse> Listar(FiltroTorneosRequest filtro, bool esAnonimo)
    {
        EstadoTorneo? estado = null;
        if(filtro.Estado != null)
        {
            estado = BuscarEstado(filtro.Estado);
        }

        var filtroRepositorio = new FiltroTorneos(
            estado,
            filtro.FechaDesde,
            filtro.FechaHasta,
            filtro.Nombre,
            filtro.SkillObligatoriaId,
            esAnonimo,
            Paginacion.AjustarLimite(filtro.Limit),
            Paginacion.AjustarDesplazamiento(filtro.Offset));
        var torneos = repositorioTorneos.Listar(filtroRepositorio);
        return torneos.Select(CrearResponse<TorneoResponse>).ToList();
    }

    public TorneoResponse Modificar(int id, TorneoRequest request)
    {
        var datos = ObtenerDatosDelTorneo(request);
        var torneo = ObtenerTorneoExistente(id);
        var organizador = ObtenerUsuarioExistente(datos.OrganizadorId);

        // El índice único de SQL Server no distingue mayúsculas: cambiar solo mayúsculas no ocupa un código ajeno (D-31).
        if(!string.Equals(torneo.Codigo, request.Codigo, StringComparison.OrdinalIgnoreCase))
        {
            ValidarCodigoDisponible(request.Codigo);
        }

        torneo.Modificar(
            request.Codigo,
            request.Nombre,
            request.Descripcion,
            datos.FechaInicio,
            datos.FechaFin,
            datos.Cupo,
            organizador,
            ObtenerFechaActual());
        repositorioTorneos.Actualizar(torneo);
        return CrearResponse<TorneoResponse>(torneo);
    }

    public TorneoResponse Cancelar(int id)
    {
        var torneo = ObtenerTorneoExistente(id);
        torneo.Cancelar();
        repositorioTorneos.Actualizar(torneo);
        return CrearResponse<TorneoResponse>(torneo);
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

    // Se compara por nombre para que un número como "1" no se tome como estado; un estado que no existe no filtra (D-35).
    private static EstadoTorneo? BuscarEstado(string estado)
    {
        foreach(var valor in Enum.GetValues<EstadoTorneo>())
        {
            if(valor.ToString() == estado)
            {
                return valor;
            }
        }

        return null;
    }

    private static DatosDelTorneo ObtenerDatosDelTorneo(TorneoRequest request)
    {
        return new DatosDelTorneo(
            DatosObligatorios.Obtener(request.FechaInicio, "La fecha de inicio es obligatoria."),
            DatosObligatorios.Obtener(request.FechaFin, "La fecha de fin es obligatoria."),
            DatosObligatorios.Obtener(request.Cupo, "El cupo es obligatorio."),
            DatosObligatorios.Obtener(request.OrganizadorId, "El organizador es obligatorio."));
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

    // Datos que el DTO declara opcionales para poder detectar cuándo se omiten.
    private sealed record DatosDelTorneo(DateOnly FechaInicio, DateOnly FechaFin, int Cupo, int OrganizadorId);
}
