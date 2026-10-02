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

        var ganador = ganadorId == batalla.Jugador1Id ? batalla.Jugador1 : batalla.Jugador2;
        var perdedor = ganadorId == batalla.Jugador1Id ? batalla.Jugador2 : batalla.Jugador1;
        var inscripcion = repositorioInscripciones.ObtenerAceptada(batalla.TorneoId, perdedor.Id);
        if(inscripcion == null)
        {
            throw new InscripcionNoActivaException();
        }

        var elo1Previo = batalla.Jugador1.Elo;
        var elo2Previo = batalla.Jugador2.Elo;
        var resultadoElo = calculadorElo.Calcular(ganador.Elo, perdedor.Elo);
        ganador.ActualizarElo(resultadoElo.NuevoEloGanador);
        perdedor.ActualizarElo(resultadoElo.NuevoEloPerdedor);
        batalla.Fallar(ganadorId, request.Comentario, batalla.Jugador1.Elo - elo1Previo, batalla.Jugador2.Elo - elo2Previo);
        inscripcion.Eliminar();

        // Todos los repositorios comparten el contexto Scoped; este es el único SaveChanges.
        repositorioBatallas.Guardar();
        return RespuestasBatallas.Detalle(batalla);
    }
}
