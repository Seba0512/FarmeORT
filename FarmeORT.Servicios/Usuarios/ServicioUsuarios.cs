using FarmeORT.Dominio.Usuarios;
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

        var rol = Enum.Parse<Rol>(request.Rol);
        var fechaActual = DateOnly.FromDateTime(reloj.GetLocalNow().DateTime);
        var contrasenaHash = hasheadorContrasenas.Hashear(request.Contrasena);
        var usuario = new Usuario(
            request.Nombre,
            request.Apellido,
            request.Email,
            request.Nickname,
            request.FechaNacimiento,
            rol,
            contrasenaHash,
            fechaActual);
        repositorioUsuarios.Agregar(usuario);
        return new UsuarioResponse();
    }
}
