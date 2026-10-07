using FarmeORT.DTOs.Rondas;

namespace FarmeORT.IServicios.Rondas;

public interface IServicioRondas
{
    RondaResponse Generar(int torneoId, int usuarioId, string rol);

    IReadOnlyList<RondaResponse> Listar(int torneoId, bool esAnonimo);

    RondaResponse ObtenerPorNumero(int torneoId, int numero, bool esAnonimo);
}
