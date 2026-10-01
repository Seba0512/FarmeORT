using FarmeORT.Dominio.Skills;

namespace FarmeORT.IRepositorios.Skills;

public interface IRepositorioSkills
{
    bool ExistePorCodigo(string codigo);

    void Agregar(Skill skill);
}
