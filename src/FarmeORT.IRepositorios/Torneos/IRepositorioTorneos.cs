using FarmeORT.Dominio.Torneos;

namespace FarmeORT.IRepositorios.Torneos;

public interface IRepositorioTorneos
{
    Torneo? ObtenerPorId(int id);

    IReadOnlyList<Torneo> Listar(FiltroTorneos filtro);

    bool ExistePorCodigo(string codigo);

    void Agregar(Torneo torneo);

    void Actualizar(Torneo torneo);

    // Torneos del organizador que no están Finalizados ni Cancelados: impiden darlo de baja o cambiarle el rol.
    bool ExisteVigenteDelOrganizador(int organizadorId);
}
