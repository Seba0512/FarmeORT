using FarmeORT.Dominio.Skills;
using FarmeORT.IRepositorios.Skills;

namespace FarmeORT.Repositorios.Skills;

internal sealed class RepositorioSkills(FarmeORTContext contexto) : IRepositorioSkills
{
    public Skill? ObtenerPorId(int id)
    {
        _ = id;
        throw new NotImplementedException();
    }

    public IReadOnlyList<Skill> Listar(FiltroSkills filtro)
    {
        _ = filtro;
        throw new NotImplementedException();
    }

    public bool ExistePorCodigo(string codigo)
    {
        _ = codigo;
        throw new NotImplementedException();
    }

    public void Agregar(Skill skill)
    {
        _ = contexto;
        _ = skill;
        throw new NotImplementedException();
    }

    public void Actualizar(Skill skill)
    {
        _ = skill;
        throw new NotImplementedException();
    }
}
