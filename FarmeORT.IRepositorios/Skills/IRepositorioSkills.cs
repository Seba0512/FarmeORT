using FarmeORT.Dominio.Skills;

namespace FarmeORT.IRepositorios.Skills;

public interface IRepositorioSkills
{
    Skill? ObtenerPorId(int id);

    bool ExistePorCodigo(string codigo);

    void Agregar(Skill skill);
}
