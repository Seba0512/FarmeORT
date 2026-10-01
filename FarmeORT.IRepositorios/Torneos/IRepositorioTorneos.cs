using FarmeORT.Dominio.Torneos;

namespace FarmeORT.IRepositorios.Torneos;

public interface IRepositorioTorneos
{
    Torneo? ObtenerPorId(int id);

    bool ExistePorCodigo(string codigo);

    void Agregar(Torneo torneo);
}
