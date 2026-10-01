using FarmeORT.DTOs.Torneos;

namespace FarmeORT.IServicios.Torneos;

public interface IServicioTorneos
{
    TorneoResponse Crear(TorneoRequest request);

    TorneoResponse ObtenerPorId(int id, bool esAnonimo);

    IReadOnlyList<TorneoResponse> Listar(FiltroTorneosRequest filtro, bool esAnonimo);

    TorneoResponse Modificar(int id, TorneoRequest request);

    TorneoResponse Cancelar(int id);
}
