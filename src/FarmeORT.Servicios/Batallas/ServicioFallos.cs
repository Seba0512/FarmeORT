using FarmeORT.Dominio.Elo;
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
        var ganadorId = DatosObligatorios.Obtener(request.GanadorId, "El ganador es obligatorio.");
        if(ganadorId < 1)
        {
            throw new DatosInvalidosException("El id del ganador debe ser positivo.");
        }

        var batalla = repositorioBatallas.ObtenerPorId(batallaId);
        if(batalla == null)
        {
            throw new BatallaNoEncontradaException();
        }

        if(batalla.Torneo.OrganizadorId != organizadorId)
        {
            throw new NoEsOrganizadorDelTorneoException();
        }

        batalla.ValidarFallo(ganadorId, request.Comentario);

        var ganador = batalla.Jugador1;
        var perdedor = batalla.Jugador2;
        if(ganadorId == batalla.Jugador2Id)
        {
            ganador = batalla.Jugador2;
            perdedor = batalla.Jugador1;
        }

        var inscripcion = repositorioInscripciones.ObtenerAceptada(batalla.TorneoId, perdedor.Id);
        if(inscripcion == null)
        {
            throw new InscripcionNoActivaException();
        }

        // Los dos ELO nuevos se calculan con los valores previos de ambos jugadores (letra 1.10).
        var eloPrevioJugador1 = batalla.Jugador1.Elo;
        var eloPrevioJugador2 = batalla.Jugador2.Elo;
        var resultadoElo = calculadorElo.Calcular(ganador.Elo, perdedor.Elo);
        ganador.ActualizarElo(resultadoElo.NuevoEloGanador);
        perdedor.ActualizarElo(resultadoElo.NuevoEloPerdedor);

        var cambioEloJugador1 = batalla.Jugador1.Elo - eloPrevioJugador1;
        var cambioEloJugador2 = batalla.Jugador2.Elo - eloPrevioJugador2;
        batalla.Fallar(ganadorId, request.Comentario, cambioEloJugador1, cambioEloJugador2);
        inscripcion.Eliminar();

        // Todos los repositorios comparten el contexto Scoped; este es el único SaveChanges.
        repositorioBatallas.Guardar();
        return RespuestasBatallas.Detalle(batalla);
    }
}
