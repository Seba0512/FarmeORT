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
        var dificultad = ObtenerDificultad(request);
        ValidarCodigoDisponible(request.Codigo);
        var skill = new Skill(request.Codigo, request.Nombre, request.Descripcion, categoria, dificultad);
        repositorioSkills.Agregar(skill);
        return CrearResponse(skill);
    }

    public SkillResponse ObtenerPorId(int id)
    {
        var skill = repositorioSkills.ObtenerPorId(id) ?? throw new SkillNoEncontradaException();
        return CrearResponse(skill);
    }

    public IReadOnlyList<SkillResponse> Listar(FiltroSkillsRequest filtro)
    {
        var categoria = filtro.Categoria is null ? (CategoriaSkill?)null : ConvertirCategoria(filtro.Categoria);
        var filtroRepositorio = new FiltroSkills(categoria, filtro.Dificultad, filtro.Activa, filtro.Nombre, filtro.Limit, filtro.Offset);
        var skills = repositorioSkills.Listar(filtroRepositorio);
        return [.. skills.Select(CrearResponse)];
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

    private static int ObtenerDificultad(SkillRequest request)
    {
        return request.Dificultad ?? throw new DatosInvalidosException("La dificultad es obligatoria.");
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
