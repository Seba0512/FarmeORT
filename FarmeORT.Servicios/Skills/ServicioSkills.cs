using FarmeORT.Dominio.Skills;
using FarmeORT.DTOs.Skills;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Skills;
using FarmeORT.IServicios.Skills;

namespace FarmeORT.Servicios.Skills;

internal sealed class ServicioSkills(IRepositorioSkills repositorioSkills) : IServicioSkills
{
    private const int LimiteMaximo = 100;

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
        var skill = ObtenerSkillExistente(id);
        return CrearResponse(skill);
    }

    public IReadOnlyList<SkillResponse> Listar(FiltroSkillsRequest filtro)
    {
        CategoriaSkill? categoria = null;
        if(filtro.Categoria != null)
        {
            categoria = ConvertirCategoria(filtro.Categoria);
        }

        var limite = filtro.Limit;
        if(limite > LimiteMaximo)
        {
            limite = LimiteMaximo;
        }

        var filtroRepositorio = new FiltroSkills(categoria, filtro.Dificultad, filtro.Activa, filtro.Nombre, limite, filtro.Offset);
        var skills = repositorioSkills.Listar(filtroRepositorio);
        return skills.Select(CrearResponse).ToList();
    }

    public SkillResponse Modificar(int id, SkillRequest request)
    {
        var categoria = ConvertirCategoria(request.Categoria);
        var dificultad = ObtenerDificultad(request);
        var skill = ObtenerSkillExistente(id);

        // El índice único de SQL Server no distingue mayúsculas: cambiar solo mayúsculas no ocupa un código ajeno.
        if(!string.Equals(skill.Codigo, request.Codigo, StringComparison.OrdinalIgnoreCase))
        {
            ValidarCodigoDisponible(request.Codigo);
        }

        skill.Modificar(request.Codigo, request.Nombre, request.Descripcion, categoria, dificultad);
        repositorioSkills.Actualizar(skill);
        return CrearResponse(skill);
    }

    public SkillResponse CambiarEstado(int id, SkillEstadoRequest request)
    {
        if(request.Activa == null)
        {
            throw new DatosInvalidosException("El estado activa es obligatorio.");
        }

        var skill = ObtenerSkillExistente(id);
        if(request.Activa.Value)
        {
            skill.Activar();
        }
        else
        {
            skill.Desactivar();
        }

        repositorioSkills.Actualizar(skill);
        return CrearResponse(skill);
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

    private void ValidarCodigoDisponible(string codigo)
    {
        if(repositorioSkills.ExistePorCodigo(codigo))
        {
            throw new CodigoSkillYaExisteException();
        }
    }

    // Se compara por nombre para que un número como "1" no se acepte como categoría.
    private static CategoriaSkill ConvertirCategoria(string categoria)
    {
        foreach(var valor in Enum.GetValues<CategoriaSkill>())
        {
            if(valor.ToString() == categoria)
            {
                return valor;
            }
        }

        throw new DatosInvalidosException("La categoría debe ser Gestual, Vocal, Baile o Meme.");
    }

    private static int ObtenerDificultad(SkillRequest request)
    {
        if(request.Dificultad == null)
        {
            throw new DatosInvalidosException("La dificultad es obligatoria.");
        }

        return request.Dificultad.Value;
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
