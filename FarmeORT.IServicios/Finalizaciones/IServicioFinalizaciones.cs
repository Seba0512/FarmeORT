using FarmeORT.DTOs.Finalizaciones;

namespace FarmeORT.IServicios.Finalizaciones;

public interface IServicioFinalizaciones
{
    FinalizacionResponse Finalizar(int torneoId, int usuarioId, string rol);
}
