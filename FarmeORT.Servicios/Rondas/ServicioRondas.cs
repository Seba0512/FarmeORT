using FarmeORT.Dominio.Batallas;
using FarmeORT.Dominio.Rondas;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.DTOs.Rondas;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Batallas;
using FarmeORT.IRepositorios.Inscripciones;
using FarmeORT.IRepositorios.Torneos;
using FarmeORT.IServicios.Rondas;

namespace FarmeORT.Servicios.Rondas;

internal sealed class ServicioRondas(
    IRepositorioTorneos repositorioTorneos,
    IRepositorioInscripciones repositorioInscripciones,
    IRepositorioBatallas repositorioBatallas,
    IEstrategiaEmparejamiento estrategiaEmparejamiento,
    ISorteo sorteo) : IServicioRondas
{
    public RondaResponse Generar(int torneoId, int usuarioId, string rol)
    {
        var torneo = repositorioTorneos.ObtenerPorId(torneoId);
        if(torneo == null)
        {
            throw new TorneoNoEncontradoException();
        }

        if(torneo.OrganizadorId != usuarioId)
        {
            throw new NoEsOrganizadorDelTorneoException();
        }

        torneo.Iniciar();
        var jugadores = repositorioInscripciones.ListarAceptadasDelTorneo(torneoId).Select(i => i.Jugador).ToList();
        var pares = estrategiaEmparejamiento.Emparejar(jugadores);
        List<Batalla> batallas = [];
        foreach(var par in pares)
        {
            var batalla = new Batalla(torneo, 1, par.Jugador1, par.Jugador2, sorteo);
            repositorioBatallas.Agregar(batalla);
            batallas.Add(batalla);
        }

        // Un solo guardado: las batallas nuevas y el cambio de estado del torneo quedan juntos o no queda nada.
        repositorioTorneos.Actualizar(torneo);
        return CrearResponse(1, batallas);
    }

    private static RondaResponse CrearResponse(int numero, IReadOnlyList<Batalla> batallas)
    {
        return new RondaResponse
        {
            Numero = numero,
            Batallas = batallas.Select(CrearResponse).ToList(),
        };
    }

    private static BatallaDeRondaResponse CrearResponse(Batalla batalla)
    {
        return new BatallaDeRondaResponse
        {
            Id = batalla.Id,
            Estado = batalla.Estado.ToString(),
            Jugador1 = CrearResponse(batalla.Jugador1),
            Jugador2 = CrearResponse(batalla.Jugador2),
        };
    }

    private static JugadorEnBatallaResponse CrearResponse(Usuario jugador)
    {
        return new JugadorEnBatallaResponse { Id = jugador.Id, Nickname = jugador.Nickname, Elo = jugador.Elo };
    }
}
