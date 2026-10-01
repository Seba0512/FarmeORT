using FarmeORT.Dominio.Skills;
using FarmeORT.DTOs.Skills;
using FarmeORT.DTOs.Torneos;
using FarmeORT.IRepositorios.Skills;
using FarmeORT.IRepositorios.Torneos;
using FarmeORT.IServicios.Torneos;

namespace FarmeORT.Servicios.Torneos;

internal sealed class ServicioSkillsObligatorias(
    IRepositorioTorneos repositorioTorneos,
    IRepositorioSkills repositorioSkills) : IServicioSkillsObligatorias
{
    public IReadOnlyList<SkillResponse> Listar(int torneoId, bool esAnonimo)
    {
        _ = torneoId;
        _ = esAnonimo;
        throw new NotImplementedException();
    }

    public SkillResponse Asociar(int torneoId, SkillObligatoriaRequest request, int organizadorId)
    {
        _ = organizadorId;
        var torneo = repositorioTorneos.ObtenerPorId(torneoId)!;
        var skill = repositorioSkills.ObtenerPorId(request.SkillId.GetValueOrDefault())!;
        torneo.AsociarSkillObligatoria(skill);
        repositorioTorneos.Actualizar(torneo);
        return CrearResponse(skill);
    }

    public void Quitar(int torneoId, int skillId, int organizadorId)
    {
        _ = torneoId;
        _ = skillId;
        _ = organizadorId;
        throw new NotImplementedException();
    }

    private static SkillResponse CrearResponse(Skill skill)
    {
        return new SkillResponse
        {
            Id = skill.Id,
            Codigo = skill.Codigo,
            Nombre = skill.Nombre,
            Descripcion = skill.Descripcion,
            Categoria = skill.Categoria.ToString(),
            Dificultad = skill.Dificultad,
            Activa = skill.Activa,
        };
    }
}
