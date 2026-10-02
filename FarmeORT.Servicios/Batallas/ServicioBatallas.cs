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

        var torneo = repositorioTorneos.ObtenerPorId(filtro.TorneoId.Value) ?? throw new TorneoNoEncontradoException();
        ValidarVisibilidad(torneo, usuarioId, rol);
        var consulta = new FiltroBatallas(
            filtro.TorneoId.Value,
            filtro.Estado == null ? null : ValoresPorNombre.Buscar<EstadoBatalla>(filtro.Estado),
            filtro.Ronda > 0 ? filtro.Ronda : null,
            filtro.JugadorId > 0 ? filtro.JugadorId : null,
            Paginacion.AjustarLimite(filtro.Limit),
            Paginacion.AjustarDesplazamiento(filtro.Offset));
        return repositorioBatallas.Listar(consulta).Select(RespuestasBatallas.Resumen).ToList();
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

    public BatallaDetalleResponse ObtenerPorId(int id, int usuarioId, string rol)
    {
        _ = repositorioBatallas.ObtenerPorId(id) ?? throw new BatallaNoEncontradaException();
        throw new NotImplementedException();
    }

    public IReadOnlyList<LanzamientoResponse> ListarLanzamientos(int id, int usuarioId, string rol)
    {
        _ = repositorioBatallas.ObtenerPorId(id) ?? throw new BatallaNoEncontradaException();
        throw new NotImplementedException();
    }
}
