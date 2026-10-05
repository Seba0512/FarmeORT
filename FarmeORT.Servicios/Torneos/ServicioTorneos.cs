using FarmeORT.Dominio.Batallas;
using FarmeORT.Dominio.Inscripciones;
using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.DTOs.Torneos;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Batallas;
using FarmeORT.IRepositorios.Inscripciones;
using FarmeORT.IRepositorios.Torneos;
using FarmeORT.IRepositorios.Usuarios;
using FarmeORT.IServicios.Torneos;
using FarmeORT.Servicios.Rondas;
using FarmeORT.Servicios.Skills;

namespace FarmeORT.Servicios.Torneos;

internal sealed class ServicioTorneos(
    IRepositorioTorneos repositorioTorneos,
    IRepositorioUsuarios repositorioUsuarios,
    IRepositorioInscripciones repositorioInscripciones,
    IRepositorioBatallas repositorioBatallas,
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

        // Un torneo recién creado todavía no tiene inscripciones.
        return CrearResponse(torneo, 0);
    }

    public TorneoDetalleResponse ObtenerPorId(int id, bool esAnonimo)
    {
        var torneo = ObtenerTorneoExistente(id);

        // Para el anónimo un torneo cancelado no existe: se responde igual que si no estuviera (D-36).
        if(esAnonimo && !torneo.EsVisibleParaAnonimos)
        {
            throw new TorneoNoEncontradoException();
        }

        // Una sola consulta de las Aceptadas da la cantidad y la lista de jugadores.
        var aceptadas = repositorioInscripciones.ListarAceptadasDelTorneo(torneo.Id);
        var batallas = repositorioBatallas.ListarDelTorneo(torneo.Id);
        return CrearDetalle(torneo, aceptadas, batallas);
    }

    public IReadOnlyList<TorneoResponse> Listar(FiltroTorneosRequest filtro, bool esAnonimo)
    {
        EstadoTorneo? estado = null;
        if(filtro.Estado != null)
        {
            estado = ValoresPorNombre.Buscar<EstadoTorneo>(filtro.Estado);
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
        if(torneos.Count == 0)
        {
            return [];
        }

        var aceptadas = repositorioInscripciones.ContarAceptadasPorTorneo(torneos.Select(t => t.Id).ToList());
        return torneos.Select(t => CrearResponse(t, aceptadas.GetValueOrDefault(t.Id))).ToList();
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
        return CrearResponse(torneo, repositorioInscripciones.ContarAceptadas(torneo.Id));
    }

    public TorneoResponse Cancelar(int id)
    {
        var torneo = ObtenerTorneoExistente(id);
        torneo.Cancelar();
        foreach(var batalla in repositorioBatallas.ListarSinResolverDelTorneo(torneo.Id))
        {
            batalla.Cancelar();
        }

        foreach(var inscripcion in repositorioInscripciones.ListarAceptadasDelTorneo(torneo.Id))
        {
            inscripcion.Cancelar();
        }

        // Un solo guardado: los tres repositorios comparten el contexto Scoped, así que el SaveChanges del
        // torneo persiste también las batallas y las inscripciones canceladas, o ninguna (D-42).
        repositorioTorneos.Actualizar(torneo);

        // La cancelación acaba de pasar todas las Aceptadas a Cancelada.
        return CrearResponse(torneo, 0);
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

    private static TorneoResponse CrearResponse(Torneo torneo, int inscripcionesAceptadas)
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
            InscripcionesAceptadas = inscripcionesAceptadas,
            OrganizadorId = torneo.OrganizadorId,
            Organizador = NombreDelOrganizador(torneo),
        };
    }

    // El detalle repite los datos del resumen y suma las obligatorias, los jugadores aceptados y el cuadro (letra 1.3).
    private static TorneoDetalleResponse CrearDetalle(
        Torneo torneo,
        IReadOnlyList<Inscripcion> aceptadas,
        IReadOnlyList<Batalla> batallas)
    {
        return new TorneoDetalleResponse
        {
            Id = torneo.Id,
            Codigo = torneo.Codigo,
            Nombre = torneo.Nombre,
            Descripcion = torneo.Descripcion,
            Estado = torneo.Estado.ToString(),
            FechaInicio = torneo.FechaInicio,
            FechaFin = torneo.FechaFin,
            Cupo = torneo.Cupo,
            InscripcionesAceptadas = aceptadas.Count,
            OrganizadorId = torneo.OrganizadorId,
            Organizador = NombreDelOrganizador(torneo),
            SkillsObligatorias = torneo.SkillsObligatorias.Select(RespuestasSkills.Crear).ToList(),
            JugadoresAceptados = aceptadas.Select(CrearJugadorAceptado).ToList(),
            Rondas = RespuestasRondas.Agrupar(batallas),
        };
    }

    private static string NombreDelOrganizador(Torneo torneo)
    {
        return $"{torneo.Organizador.Nombre} {torneo.Organizador.Apellido}";
    }

    private static JugadorAceptadoResponse CrearJugadorAceptado(Inscripcion inscripcion)
    {
        return new JugadorAceptadoResponse
        {
            JugadorId = inscripcion.JugadorId,
            Nickname = inscripcion.Jugador.Nickname,
            Elo = inscripcion.Jugador.Elo,
        };
    }

    // Datos que el DTO declara opcionales para poder detectar cuándo se omiten.
    private sealed record DatosDelTorneo(DateOnly FechaInicio, DateOnly FechaFin, int Cupo, int OrganizadorId);
}
