using FarmeORT.Dominio.Skills;
using FarmeORT.IRepositorios.Skills;

namespace FarmeORT.Repositorios.Skills;

internal sealed class RepositorioSkills(FarmeORTContext contexto) : IRepositorioSkills
{
    public Skill? ObtenerPorId(int id)
    {
        return contexto.Skills.SingleOrDefault(s => s.Id == id);
    }

    public IReadOnlyList<Skill> Listar(FiltroSkills filtro)
    {
        var consulta = contexto.Skills.AsQueryable();
        if(filtro.Categoria is { } categoria)
        {
            consulta = consulta.Where(s => s.Categoria == categoria);
        }

        return consulta
            .OrderBy(s => s.Id)
            .ToList();
    }

    // SQL Server no distingue mayúsculas y SQLite sí: se compara en minúscula para responder igual en los dos.
    public bool ExistePorCodigo(string codigo)
    {
        var codigoBuscado = codigo.ToLower();
        return contexto.Skills.Any(s => s.Codigo.ToLower() == codigoBuscado);
    }

    public void Agregar(Skill skill)
    {
        contexto.Skills.Add(skill);
        contexto.SaveChanges();
    }

    public void Actualizar(Skill skill)
    {
        _ = skill;
        throw new NotImplementedException();
    }
}
