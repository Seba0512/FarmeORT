using FarmeORT.DTOs.Usuarios;
using FarmeORT.IRepositorios.Usuarios;
using FarmeORT.IServicios.Seguridad;
using FarmeORT.IServicios.Usuarios;

namespace FarmeORT.Servicios.Usuarios;

internal sealed class ServicioUsuarios(
    IRepositorioUsuarios repositorioUsuarios,
    IHasheadorContrasenas hasheadorContrasenas,
    TimeProvider reloj) : IServicioUsuarios
{
    public UsuarioResponse Crear(UsuarioAdminRequest request)
    {
        _ = repositorioUsuarios;
        _ = hasheadorContrasenas;
        _ = reloj;
        throw new NotImplementedException();
    }
}
