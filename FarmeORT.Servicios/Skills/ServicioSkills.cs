using FarmeORT.Dominio.Skills;
using FarmeORT.DTOs.Skills;
using FarmeORT.IRepositorios.Skills;
using FarmeORT.IServicios.Skills;

namespace FarmeORT.Servicios.Skills;

internal sealed class ServicioSkills(IRepositorioSkills repositorioSkills) : IServicioSkills
{
    public SkillResponse Crear(SkillRequest request)
    {
        var categoria = Enum.Parse<CategoriaSkill>(request.Categoria);
        var skill = new Skill(request.Codigo, request.Nombre, request.Descripcion, categoria, request.Dificultad.GetValueOrDefault());
        repositorioSkills.Agregar(skill);
        return CrearResponse(skill);
    }

    public SkillResponse ObtenerPorId(int id)
    {
        _ = id;
        throw new NotImplementedException();
    }

    public IReadOnlyList<SkillResponse> Listar(FiltroSkillsRequest filtro)
    {
        _ = filtro;
        throw new NotImplementedException();
    }

    public SkillResponse Modificar(int id, SkillRequest request)
    {
        _ = id;
        _ = request;
        throw new NotImplementedException();
    }

    public SkillResponse CambiarEstado(int id, SkillEstadoRequest request)
    {
        _ = id;
        _ = request;
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
