using FarmeORT.Dominio.Skills;
using FarmeORT.DTOs.Skills;

namespace FarmeORT.Servicios.Skills;

// El catálogo, las skills obligatorias y el detalle del torneo muestran la skill con el mismo formato.
internal static class RespuestasSkills
{
    public static SkillResponse Crear(Skill skill)
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
