using FarmeORT.Dominio.Torneos;

namespace FarmeORT.IRepositorios.Torneos;

public interface IRepositorioTorneos
{
    bool ExistePorCodigo(string codigo);

    void Agregar(Torneo torneo);
}
