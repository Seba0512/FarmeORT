using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;
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
        var torneo = ObtenerTorneoExistente(torneoId);
        PermisosDelTorneo.ValidarOrganizadorOAdministrador(torneo, usuarioId, rol);

        // Primero el estado: un torneo Abierto, Finalizado o Cancelado no está en curso, tenga o no campeón (D-64).
        if(torneo.Estado != EstadoTorneo.EnCurso)
        {
            throw new TorneoNoEnCursoException();
        }

        var campeon = ObtenerCampeon(torneoId);
        torneo.Finalizar(campeon);
        repositorioTorneos.Actualizar(torneo);
        return CrearResponse(torneo, campeon);
    }

    private Torneo ObtenerTorneoExistente(int torneoId)
    {
        var torneo = repositorioTorneos.ObtenerPorId(torneoId);
        if(torneo == null)
        {
            throw new TorneoNoEncontradoException();
        }

        return torneo;
    }

    private Usuario ObtenerCampeon(int torneoId)
    {
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

        return sinEliminar[0].Jugador;
    }

    private static FinalizacionResponse CrearResponse(Torneo torneo, Usuario campeon)
    {
        return new FinalizacionResponse
        {
            TorneoId = torneo.Id,
            Estado = torneo.Estado.ToString(),
            Campeon = new JugadorBatallaResponse { Id = campeon.Id, Nickname = campeon.Nickname, Elo = campeon.Elo },
        };
    }
}
