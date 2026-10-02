using FarmeORT.DTOs.Batallas;
using FarmeORT.DTOs.Finalizaciones;
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
        _ = repositorioBatallas;
        var torneo = repositorioTorneos.ObtenerPorId(torneoId)!;
        var campeon = repositorioInscripciones.ListarAceptadasDelTorneo(torneoId).Single().Jugador;
        return new FinalizacionResponse
        {
            TorneoId = torneo.Id,
            Estado = "Finalizado",
            Campeon = new JugadorBatallaResponse { Id = campeon.Id, Nickname = campeon.Nickname, Elo = campeon.Elo },
        };
    }
}
