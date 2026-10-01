using FarmeORT.Dominio.Skills;

namespace FarmeORT.IRepositorios.Skills;

public interface IRepositorioSkills
{
    Skill? ObtenerPorId(int id);

    IReadOnlyList<Skill> Listar(FiltroSkills filtro);

    bool ExistePorCodigo(string codigo);

    void Agregar(Skill skill);
}
