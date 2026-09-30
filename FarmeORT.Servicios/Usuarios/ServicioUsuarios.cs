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
        ReglaContrasena.Validar(request.Contrasena);
        var rol = ConvertirRol(request.Rol);

        if(repositorioUsuarios.ObtenerPorEmail(request.Email) is not null)
        {
            throw new EmailYaExisteException();
        }

        if(repositorioUsuarios.ExistePorNickname(request.Nickname))
        {
            throw new NicknameYaExisteException();
        }

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
        return CrearResponse(usuario);
    }

    public UsuarioResponse ObtenerPorId(int id)
    {
        var usuario = repositorioUsuarios.ObtenerPorId(id) ?? throw new UsuarioNoEncontradoException();
        return CrearResponse(usuario);
    }

    public IReadOnlyList<UsuarioResponse> Listar(FiltroUsuariosRequest filtro)
    {
        throw new NotImplementedException();
    }

    private static Rol ConvertirRol(string rol)
    {
        if(!Enum.TryParse<Rol>(rol, out var rolConvertido) || !Enum.IsDefined(rolConvertido))
        {
            throw new DatosInvalidosException("El rol debe ser Jugador, Organizador o Administrador.");
        }

        return rolConvertido;
    }

    private static UsuarioResponse CrearResponse(Usuario usuario)
    {
        return new UsuarioResponse
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            Email = usuario.Email,
            Nickname = usuario.Nickname,
            FechaNacimiento = usuario.FechaNacimiento,
            Rol = usuario.Rol.ToString(),
            Elo = usuario.Rol == Rol.Jugador ? usuario.Elo : null,
        };
    }
}
