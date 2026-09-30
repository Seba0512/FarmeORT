using FarmeORT.DTOs.Sesiones;

namespace FarmeORT.IServicios.Sesiones;

public interface IServicioSesiones
{
    SesionResponse IniciarSesion(LoginRequest request);

    void CerrarSesion(Guid token);
}
