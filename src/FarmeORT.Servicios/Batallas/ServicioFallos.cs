using FarmeORT.Dominio.Batallas;
using FarmeORT.Dominio.Elo;
using FarmeORT.Dominio.Inscripciones;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.DTOs.Batallas;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Batallas;
using FarmeORT.IRepositorios.Inscripciones;
using FarmeORT.IServicios.Batallas;

namespace FarmeORT.Servicios.Batallas;

internal sealed class ServicioFallos(
    IRepositorioBatallas repositorioBatallas,
    IRepositorioInscripciones repositorioInscripciones,
    ICalculadorElo calculadorElo) : IServicioFallos
{
    public BatallaDetalleResponse Fallar(int batallaId, int organizadorId, FalloRequest request)
    {
        var ganadorId = ObtenerGanadorId(request);
        var batalla = ObtenerBatallaDelOrganizador(batallaId, organizadorId);
        batalla.ValidarFallo(ganadorId, request.Comentario);

        var ganador = ganadorId == batalla.Jugador1Id ? batalla.Jugador1 : batalla.Jugador2;
        var perdedor = ganadorId == batalla.Jugador1Id ? batalla.Jugador2 : batalla.Jugador1;
        var inscripcionDelPerdedor = ObtenerInscripcionAceptada(batalla.TorneoId, perdedor.Id);

        var cambiosElo = ActualizarElo(batalla, ganador, perdedor);
        batalla.Fallar(ganadorId, request.Comentario, cambiosElo.Jugador1, cambiosElo.Jugador2);
        inscripcionDelPerdedor.Eliminar();

        // Todos los repositorios comparten el contexto Scoped; este es el único SaveChanges.
        repositorioBatallas.Guardar();
        return RespuestasBatallas.Detalle(batalla);
    }

    private static int ObtenerGanadorId(FalloRequest request)
    {
        var ganadorId = DatosObligatorios.Obtener(request.GanadorId, "El ganador es obligatorio.");
        if(ganadorId < 1)
        {
            throw new DatosInvalidosException("El id del ganador debe ser positivo.");
        }

        return ganadorId;
    }

    private Batalla ObtenerBatallaDelOrganizador(int batallaId, int organizadorId)
    {
        var batalla = repositorioBatallas.ObtenerPorId(batallaId);
        if(batalla == null)
        {
            throw new BatallaNoEncontradaException();
        }

        if(batalla.Torneo.OrganizadorId != organizadorId)
        {
            throw new NoEsOrganizadorDelTorneoException();
        }

        return batalla;
    }

    private Inscripcion ObtenerInscripcionAceptada(int torneoId, int jugadorId)
    {
        var inscripcion = repositorioInscripciones.ObtenerAceptada(torneoId, jugadorId);
        if(inscripcion == null)
        {
            throw new InscripcionNoActivaException();
        }

        return inscripcion;
    }

    // Los dos ELO nuevos se calculan con los valores previos de ambos jugadores (letra 1.10).
    private CambiosElo ActualizarElo(Batalla batalla, Usuario ganador, Usuario perdedor)
    {
        var eloPrevioJugador1 = batalla.Jugador1.Elo;
        var eloPrevioJugador2 = batalla.Jugador2.Elo;
        var resultadoElo = calculadorElo.Calcular(ganador.Elo, perdedor.Elo);
        ganador.ActualizarElo(resultadoElo.NuevoEloGanador);
        perdedor.ActualizarElo(resultadoElo.NuevoEloPerdedor);
        return new CambiosElo(batalla.Jugador1.Elo - eloPrevioJugador1, batalla.Jugador2.Elo - eloPrevioJugador2);
    }

    private sealed record CambiosElo(int Jugador1, int Jugador2);
}
