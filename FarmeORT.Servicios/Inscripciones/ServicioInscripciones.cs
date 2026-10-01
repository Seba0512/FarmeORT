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
        _ = id;
        _ = usuarioId;
        _ = rol;
        throw new NotImplementedException();
    }

    public InscripcionResponse Cancelar(int id, int jugadorId)
    {
        _ = id;
        _ = jugadorId;
        throw new NotImplementedException();
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
