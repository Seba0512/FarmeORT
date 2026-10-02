using FarmeORT.DTOs.Batallas;
using FarmeORT.DTOs.Finalizaciones;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Batallas;
using FarmeORT.IRepositorios.Inscripciones;
using FarmeORT.IRepositorios.Torneos;
using FarmeORT.IServicios.Finalizaciones;
using FarmeORT.Servicios.Torneos;

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

        PermisosDelTorneo.ValidarOrganizadorOAdministrador(torneo, usuarioId, rol);

        if(repositorioBatallas.ListarSinResolverDelTorneo(torneoId).Count > 0)
        {
            throw new TorneoConBatallasSinResolverException();
        }

        // El campeón es el único jugador que sigue con inscripción Aceptada: el resto quedó Eliminado al fallar.
        var sinEliminar = repositorioInscripciones.ListarAceptadasDelTorneo(torneoId);
        if(sinEliminar.Count != 1)
        {
            throw new TorneoSinCampeonException();
        }

        var campeon = sinEliminar[0].Jugador;
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
