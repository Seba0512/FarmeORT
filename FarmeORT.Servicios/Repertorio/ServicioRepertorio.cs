using FarmeORT.Dominio.Usuarios;
using FarmeORT.DTOs.Repertorio;
using FarmeORT.IRepositorios.Skills;
using FarmeORT.IRepositorios.Usuarios;
using FarmeORT.IServicios.Repertorio;

namespace FarmeORT.Servicios.Repertorio;

internal sealed class ServicioRepertorio(
    IRepositorioUsuarios repositorioUsuarios,
    IRepositorioSkills repositorioSkills,
    TimeProvider reloj) : IServicioRepertorio
{
    public IReadOnlyList<SkillDeclaradaResponse> Listar(int jugadorId)
    {
        _ = jugadorId;
        throw new NotImplementedException();
    }

    public SkillDeclaradaResponse Declarar(int jugadorId, SkillDeclaradaRequest request)
    {
        var jugador = repositorioUsuarios.ObtenerConRepertorio(jugadorId)!;
        var skill = repositorioSkills.ObtenerPorId(request.SkillId.GetValueOrDefault())!;
        var declarada = jugador.DeclararSkill(skill, request.Nivel.GetValueOrDefault(), ObtenerFechaActual());
        repositorioUsuarios.GuardarRepertorio(jugador);
        return CrearResponse(declarada);
    }

    public void Quitar(int jugadorId, int skillId)
    {
        _ = jugadorId;
        _ = skillId;
        throw new NotImplementedException();
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
