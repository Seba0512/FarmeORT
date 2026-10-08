using FarmeORT.Dominio.Batallas;
using FarmeORT.Dominio.Skills;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.DTOs.Batallas;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Batallas;
using FarmeORT.IRepositorios.Skills;
using FarmeORT.IRepositorios.Usuarios;
using FarmeORT.IServicios.Batallas;

namespace FarmeORT.Servicios.Batallas;

internal sealed class ServicioLanzamientos(
    IRepositorioBatallas repositorioBatallas,
    IRepositorioUsuarios repositorioUsuarios,
    IRepositorioSkills repositorioSkills,
    TimeProvider reloj) : IServicioLanzamientos
{
    public LanzamientoResultadoResponse Lanzar(int batallaId, int jugadorId, LanzamientoRequest request)
    {
        var skillId = ObtenerSkillId(request);
        var batalla = ObtenerBatallaExistente(batallaId);
        var jugador = ObtenerJugadorExistente(jugadorId);
        var skill = ObtenerSkillExistente(skillId);

        var lanzamiento = batalla.Lanzar(jugador, skill, reloj.GetUtcNow());
        repositorioBatallas.Guardar();
        return new LanzamientoResultadoResponse
        {
            Turno = lanzamiento.Turno,
            Estado = batalla.Estado.ToString(),
            JugadorTurnoId = batalla.JugadorTurnoId,
        };
    }

    private static int ObtenerSkillId(LanzamientoRequest request)
    {
        var skillId = DatosObligatorios.Obtener(request.SkillId, "La skill es obligatoria.");
        if(skillId < 1)
        {
            throw new DatosInvalidosException("El id de la skill debe ser positivo.");
        }

        return skillId;
    }

    private Batalla ObtenerBatallaExistente(int batallaId)
    {
        var batalla = repositorioBatallas.ObtenerPorId(batallaId);
        if(batalla == null)
        {
            throw new BatallaNoEncontradaException();
        }

        return batalla;
    }

    // Se trae con el repertorio para controlar que la skill sea suya.
    private Usuario ObtenerJugadorExistente(int jugadorId)
    {
        var jugador = repositorioUsuarios.ObtenerConRepertorio(jugadorId);
        if(jugador == null)
        {
            throw new UsuarioNoEncontradoException();
        }

        return jugador;
    }

    private Skill ObtenerSkillExistente(int skillId)
    {
        var skill = repositorioSkills.ObtenerPorId(skillId);
        if(skill == null)
        {
            throw new SkillNoEncontradaException();
        }

        return skill;
    }
}
