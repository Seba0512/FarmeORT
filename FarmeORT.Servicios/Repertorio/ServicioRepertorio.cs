using FarmeORT.Dominio.Skills;
using FarmeORT.Dominio.Usuarios;
using FarmeORT.DTOs.Repertorio;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Skills;
using FarmeORT.IRepositorios.Usuarios;
using FarmeORT.IServicios.Repertorio;

namespace FarmeORT.Servicios.Repertorio;

internal sealed class ServicioRepertorio(
    IRepositorioUsuarios repositorioUsuarios,
    IRepositorioSkills repositorioSkills,
    TimeProvider reloj) : IServicioRepertorio
{
    // Se ordena por skill para que el resultado no dependa del orden en que la base devuelve las filas.
    public IReadOnlyList<SkillDeclaradaResponse> Listar(int jugadorId)
    {
        var jugador = ObtenerJugadorExistente(jugadorId);
        return jugador.Repertorio
            .OrderBy(d => d.SkillId)
            .Select(CrearResponse)
            .ToList();
    }

    public SkillDeclaradaResponse Declarar(int jugadorId, SkillDeclaradaRequest request)
    {
        var skillId = DatosObligatorios.Obtener(request.SkillId, "La skill es obligatoria.");
        var nivel = DatosObligatorios.Obtener(request.Nivel, "El nivel es obligatorio.");
        var jugador = ObtenerJugadorExistente(jugadorId);
        var skill = ObtenerSkillExistente(skillId);
        var declarada = jugador.DeclararSkill(skill, nivel, ObtenerFechaActual());
        repositorioUsuarios.GuardarRepertorio(jugador);
        return CrearResponse(declarada);
    }

    public void Quitar(int jugadorId, int skillId)
    {
        var jugador = ObtenerJugadorExistente(jugadorId);
        jugador.QuitarSkill(skillId);
        repositorioUsuarios.GuardarRepertorio(jugador);
    }

    // El token es válido, pero el jugador pudo haberse dado de baja después de iniciar sesión.
    private Usuario ObtenerJugadorExistente(int id)
    {
        var jugador = repositorioUsuarios.ObtenerConRepertorio(id);
        if(jugador == null)
        {
            throw new UsuarioNoEncontradoException();
        }

        return jugador;
    }

    private Skill ObtenerSkillExistente(int id)
    {
        var skill = repositorioSkills.ObtenerPorId(id);
        if(skill == null)
        {
            throw new SkillNoEncontradaException();
        }

        return skill;
    }

    private DateOnly ObtenerFechaActual()
    {
        return DateOnly.FromDateTime(reloj.GetLocalNow().DateTime);
    }

    private static SkillDeclaradaResponse CrearResponse(SkillDeclarada declarada)
    {
        return new SkillDeclaradaResponse
        {
            SkillId = declarada.SkillId,
            Codigo = declarada.Skill.Codigo,
            Nombre = declarada.Skill.Nombre,
            Categoria = declarada.Skill.Categoria.ToString(),
            Activa = declarada.Skill.Activa,
            Nivel = declarada.Nivel,
            FechaDeclaracion = declarada.FechaDeclaracion,
        };
    }
}
