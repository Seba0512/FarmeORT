using FarmeORT.DTOs.Torneos;

namespace FarmeORT.IServicios.Torneos;

public interface IServicioTorneos
{
    TorneoResponse Crear(TorneoRequest request);

    TorneoResponse ObtenerPorId(int id, bool esAnonimo);
}
