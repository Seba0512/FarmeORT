using FarmeORT.DTOs.Usuarios;
using FarmeORT.Excepciones;
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
        if(repositorioUsuarios.ObtenerPorEmail(request.Email) is not null)
        {
            throw new EmailYaExisteException();
        }

        if(repositorioUsuarios.ExistePorNickname(request.Nickname))
        {
            throw new NicknameYaExisteException();
        }

        _ = hasheadorContrasenas;
        _ = reloj;
        throw new NotImplementedException();
    }
}
