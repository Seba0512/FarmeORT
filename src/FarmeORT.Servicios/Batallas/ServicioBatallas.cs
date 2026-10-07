using FarmeORT.Dominio.Batallas;
using FarmeORT.Dominio.Torneos;
using FarmeORT.DTOs.Batallas;
using FarmeORT.DTOs.Usuarios;
using FarmeORT.Excepciones;
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
        if(filtro.TorneoId == null || filtro.TorneoId <= 0)
        {
            throw new DatosInvalidosException("El torneoId es obligatorio y debe ser positivo.");
        }

        var torneoId = filtro.TorneoId.Value;
        var torneo = repositorioTorneos.ObtenerPorId(torneoId);
        if(torneo == null)
        {
            throw new TorneoNoEncontradoException();
        }

        ValidarVisibilidad(torneo, usuarioId, rol);
        var consulta = CrearConsulta(torneoId, filtro);
        return repositorioBatallas.Listar(consulta).Select(RespuestasBatallas.Resumen).ToList();
    }

    public BatallaDetalleResponse ObtenerPorId(int id, int usuarioId, string rol)
    {
        var batalla = ObtenerAutorizada(id, usuarioId, rol);
        return RespuestasBatallas.Detalle(batalla);
    }

    public IReadOnlyList<LanzamientoResponse> ListarLanzamientos(int id, int usuarioId, string rol)
    {
        var batalla = ObtenerAutorizada(id, usuarioId, rol);
        return RespuestasBatallas.Lanzamientos(batalla);
    }

    // Un filtro opcional con un valor inválido no se aplica, en lugar de rechazar el pedido (D-35).
    private static FiltroBatallas CrearConsulta(int torneoId, FiltroBatallasRequest filtro)
    {
        var estado = ValoresPorNombre.Buscar<EstadoBatalla>(filtro.Estado);

        var ronda = filtro.Ronda > 0 ? filtro.Ronda : null;
        var jugadorId = filtro.JugadorId > 0 ? filtro.JugadorId : null;

        return new FiltroBatallas(
            torneoId,
            estado,
            ronda,
            jugadorId,
            Paginacion.AjustarLimite(filtro.Limit),
            Paginacion.AjustarDesplazamiento(filtro.Offset));
    }

    private Batalla ObtenerAutorizada(int id, int usuarioId, string rol)
    {
        var batalla = repositorioBatallas.ObtenerPorId(id);
        if(batalla == null)
        {
            throw new BatallaNoEncontradaException();
        }

        ValidarVisibilidad(batalla.Torneo, usuarioId, rol);
        return batalla;
    }

    private void ValidarVisibilidad(Torneo torneo, int usuarioId, string rol)
    {
        if(rol == Roles.Jugador && !repositorioInscripciones.ExisteDelJugador(torneo.Id, usuarioId))
        {
            throw new JugadorNoInscriptoEnTorneoException();
        }

        if(rol == Roles.Organizador && torneo.OrganizadorId != usuarioId)
        {
            throw new NoEsOrganizadorDelTorneoException();
        }
    }
}
