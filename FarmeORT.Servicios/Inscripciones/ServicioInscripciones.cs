using FarmeORT.Dominio.Inscripciones;
using FarmeORT.Dominio.Torneos;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.DTOs.Inscripciones;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Inscripciones;
using FarmeORT.IRepositorios.Torneos;
using FarmeORT.IRepositorios.Usuarios;
using FarmeORT.IServicios.Inscripciones;

namespace FarmeORT.Servicios.Inscripciones;

internal sealed class ServicioInscripciones(
    IRepositorioInscripciones repositorioInscripciones,
    IRepositorioTorneos repositorioTorneos,
    IRepositorioUsuarios repositorioUsuarios,
    TimeProvider reloj) : IServicioInscripciones
{
    public InscripcionResponse Inscribir(int jugadorId, InscripcionRequest request)
    {
        var torneoId = DatosObligatorios.Obtener(request.TorneoId, "El torneo es obligatorio.");
        var torneo = ObtenerTorneoExistente(torneoId);
        var jugador = ObtenerJugadorExistente(jugadorId);
        if(repositorioInscripciones.ExisteAceptada(torneoId, jugadorId))
        {
            throw new InscripcionYaExisteException();
        }

        if(repositorioInscripciones.ContarAceptadas(torneoId) >= torneo.Cupo)
        {
            throw new TorneoCompletoException();
        }

        var inscripcion = new Inscripcion(torneo, jugador, reloj.GetLocalNow().DateTime);
        repositorioInscripciones.Agregar(inscripcion);
        return CrearResponse(inscripcion);
    }

    public IReadOnlyList<InscripcionResponse> Listar(FiltroInscripcionesRequest filtro, int usuarioId, string rol)
    {
        _ = filtro;
        _ = usuarioId;
        _ = rol;
        throw new NotImplementedException();
    }

    public InscripcionResponse ObtenerPorId(int id, int usuarioId, string rol)
    {
        _ = usuarioId;
        _ = rol;
        var inscripcion = ObtenerInscripcionExistente(id);
        return CrearResponse(inscripcion);
    }

    public InscripcionResponse Cancelar(int id, int jugadorId)
    {
        var inscripcion = ObtenerInscripcionExistente(id);
        if(inscripcion.JugadorId != jugadorId)
        {
            throw new NoEsDuenoDeLaInscripcionException();
        }

        // El jugador se puede retirar solo antes de que el torneo empiece.
        if(inscripcion.Torneo.Estado != EstadoTorneo.Abierto)
        {
            throw new TorneoNoAbiertoException();
        }

        inscripcion.Cancelar();
        repositorioInscripciones.Actualizar(inscripcion);
        return CrearResponse(inscripcion);
    }

    private Inscripcion ObtenerInscripcionExistente(int id)
    {
        var inscripcion = repositorioInscripciones.ObtenerPorId(id);
        if(inscripcion == null)
        {
            throw new InscripcionNoEncontradaException();
        }

        return inscripcion;
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

    // Se trae con el repertorio para controlar las skills obligatorias del torneo.
    private Usuario ObtenerJugadorExistente(int jugadorId)
    {
        var jugador = repositorioUsuarios.ObtenerConRepertorio(jugadorId);
        if(jugador == null)
        {
            throw new UsuarioNoEncontradoException();
        }

        return jugador;
    }

    private static InscripcionResponse CrearResponse(Inscripcion inscripcion)
    {
        return new InscripcionResponse
        {
            Id = inscripcion.Id,
            JugadorId = inscripcion.JugadorId,
            Nickname = inscripcion.Jugador.Nickname,
            Elo = inscripcion.Jugador.Elo,
            TorneoId = inscripcion.TorneoId,
            Torneo = inscripcion.Torneo.Nombre,
            Estado = inscripcion.Estado.ToString(),
            Fecha = inscripcion.Fecha,
        };
    }
}
