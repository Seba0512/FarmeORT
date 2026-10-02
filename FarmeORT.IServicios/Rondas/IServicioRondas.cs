using FarmeORT.DTOs.Rondas;

namespace FarmeORT.IServicios.Rondas;

public interface IServicioRondas
{
    RondaResponse Generar(int torneoId, int usuarioId, string rol);
}
