using FarmeORT.DTOs.Usuarios;

namespace FarmeORT.IServicios.Usuarios;

public interface IServicioUsuarios
{
    UsuarioResponse Crear(UsuarioAdminRequest request);
}
