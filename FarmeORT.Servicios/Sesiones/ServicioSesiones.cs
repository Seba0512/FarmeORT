using FarmeORT.DTOs.Sesiones;
using FarmeORT.IRepositorios.Usuarios;
using FarmeORT.IServicios.Sesiones;

namespace FarmeORT.Servicios.Sesiones;

internal sealed class ServicioSesiones(IRepositorioUsuarios repositorioUsuarios) : IServicioSesiones
{
    public SesionResponse IniciarSesion(LoginRequest request)
    {
        _ = repositorioUsuarios;
        _ = request;
        throw new NotImplementedException();
    }
}
