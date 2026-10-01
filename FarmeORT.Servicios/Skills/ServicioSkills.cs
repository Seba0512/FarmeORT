using FarmeORT.Dominio.Skills;
using FarmeORT.DTOs.Skills;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Skills;
using FarmeORT.IServicios.Skills;

namespace FarmeORT.Servicios.Skills;

internal sealed class ServicioSkills(IRepositorioSkills repositorioSkills) : IServicioSkills
{
    public SkillResponse Crear(SkillRequest request)
    {
        var categoria = ConvertirCategoria(request.Categoria);
        ValidarCodigoDisponible(request.Codigo);
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

    private void ValidarCodigoDisponible(string codigo)
    {
        if(repositorioSkills.ExistePorCodigo(codigo))
        {
            throw new CodigoSkillYaExisteException();
        }
    }

    // Se compara contra los nombres para que un número como "1" no se acepte como categoría.
    private static CategoriaSkill ConvertirCategoria(string categoria)
    {
        if(!Enum.GetNames<CategoriaSkill>().Contains(categoria))
        {
            throw new DatosInvalidosException("La categoría debe ser Gestual, Vocal, Baile o Meme.");
        }

        return Enum.Parse<CategoriaSkill>(categoria);
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
