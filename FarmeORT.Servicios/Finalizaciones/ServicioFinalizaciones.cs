using FarmeORT.DTOs.Batallas;
using FarmeORT.DTOs.Finalizaciones;
using FarmeORT.DTOs.Usuarios;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Batallas;
using FarmeORT.IRepositorios.Inscripciones;
using FarmeORT.IRepositorios.Torneos;
using FarmeORT.IServicios.Finalizaciones;

namespace FarmeORT.Servicios.Finalizaciones;

internal sealed class ServicioFinalizaciones(
    IRepositorioTorneos repositorioTorneos,
    IRepositorioInscripciones repositorioInscripciones,
    IRepositorioBatallas repositorioBatallas) : IServicioFinalizaciones
{
    public FinalizacionResponse Finalizar(int torneoId, int usuarioId, string rol)
    {
        var torneo = repositorioTorneos.ObtenerPorId(torneoId);
        if(torneo == null)
        {
            throw new TorneoNoEncontradoException();
        }

        // Un Administrador puede finalizar aunque no sea el organizador asignado (D-63).
        if(rol != Roles.Administrador && torneo.OrganizadorId != usuarioId)
        {
            throw new NoEsOrganizadorDelTorneoException();
        }

        if(repositorioBatallas.ListarSinResolverDelTorneo(torneoId).Count > 0)
        {
            throw new TorneoConBatallasSinResolverException();
        }

        var campeon = repositorioInscripciones.ListarAceptadasDelTorneo(torneoId).Single().Jugador;
        torneo.Finalizar(campeon);
        repositorioTorneos.Actualizar(torneo);
        return new FinalizacionResponse
        {
            TorneoId = torneo.Id,
            Estado = torneo.Estado.ToString(),
            Campeon = new JugadorBatallaResponse { Id = campeon.Id, Nickname = campeon.Nickname, Elo = campeon.Elo },
        };
    }
}
