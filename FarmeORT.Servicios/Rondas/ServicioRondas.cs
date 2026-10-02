using FarmeORT.Dominio.Batallas;
using FarmeORT.Dominio.Rondas;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.DTOs.Rondas;
using FarmeORT.DTOs.Usuarios;
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

        // El filtro solo sabe el rol: si un Organizador es el asignado a este torneo lo decide el servicio (D-40).
        // Un Administrador puede generarla aunque no lo sea (D-63).
        if(rol != Roles.Administrador && torneo.OrganizadorId != usuarioId)
        {
            throw new NoEsOrganizadorDelTorneoException();
        }

        var jugadores = repositorioInscripciones.ListarAceptadasDelTorneo(torneoId).Select(i => i.Jugador).ToList();
        torneo.Iniciar(jugadores.Count);
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

    // Una ronda son las batallas del torneo con el mismo número; el repositorio ya las trae ordenadas (D-62).
    public IReadOnlyList<RondaResponse> Listar(int torneoId, bool esAnonimo)
    {
        if(repositorioTorneos.ObtenerPorId(torneoId) == null)
        {
            throw new TorneoNoEncontradoException();
        }

        var batallas = repositorioBatallas.ListarDelTorneo(torneoId);
        return batallas
            .GroupBy(b => b.NumeroRonda)
            .Select(ronda => CrearResponse(ronda.Key, ronda.ToList()))
            .ToList();
    }

    public RondaResponse ObtenerPorNumero(int torneoId, int numero, bool esAnonimo)
    {
        _ = torneoId;
        _ = numero;
        _ = esAnonimo;
        throw new NotImplementedException();
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
