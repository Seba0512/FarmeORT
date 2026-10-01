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
    public UsuarioResponse RegistrarJugador(RegistroJugadorRequest request)
    {
        var fechaNacimiento = ObtenerFechaNacimiento(request);
        ReglaContrasena.Validar(request.Contrasena);
        return CrearUsuario(request, Rol.Jugador, fechaNacimiento);
    }

    public UsuarioResponse Crear(UsuarioAdminRequest request)
    {
        ReglaContrasena.Validar(request.Contrasena);
        var rol = ConvertirRol(request.Rol);
        var fechaNacimiento = ObtenerFechaNacimiento(request);
        return CrearUsuario(request, rol, fechaNacimiento);
    }

    public UsuarioResponse ObtenerPorId(int id)
    {
        var usuario = repositorioUsuarios.ObtenerPorId(id) ?? throw new UsuarioNoEncontradoException();
        return CrearResponse(usuario);
    }

    public IReadOnlyList<UsuarioResponse> Listar(FiltroUsuariosRequest filtro)
    {
        var filtroRepositorio = new FiltroUsuarios(filtro.Nombre, filtro.Apellido, filtro.Nickname, filtro.Limit, filtro.Offset);
        var usuarios = repositorioUsuarios.Listar(filtroRepositorio);
        return [.. usuarios.Select(CrearResponse)];
    }

    public UsuarioResponse Modificar(int id, UsuarioAdminRequest request, int usuarioActualId)
    {
        if(id == usuarioActualId)
        {
            throw new OperacionSobreSiMismoException();
        }

        ReglaContrasena.Validar(request.Contrasena);
        var rol = ConvertirRol(request.Rol);
        var fechaNacimiento = ObtenerFechaNacimiento(request);
        var usuario = repositorioUsuarios.ObtenerPorId(id) ?? throw new UsuarioNoEncontradoException();
        if(!MismoTexto(usuario.Email, request.Email))
        {
            ValidarEmailDisponible(request.Email);
        }

        if(!MismoTexto(usuario.Nickname, request.Nickname))
        {
            ValidarNicknameDisponible(request.Nickname);
        }

        var fechaActual = ObtenerFechaActual();
        var contrasenaHash = hasheadorContrasenas.Hashear(request.Contrasena);
        usuario.Modificar(
            request.Nombre,
            request.Apellido,
            request.Email,
            request.Nickname,
            fechaNacimiento,
            rol,
            contrasenaHash,
            fechaActual);
        repositorioUsuarios.Actualizar(usuario);
        return CrearResponse(usuario);
    }

    public void Eliminar(int id, int usuarioActualId)
    {
        if(id == usuarioActualId)
        {
            throw new OperacionSobreSiMismoException();
        }

        throw new NotImplementedException();
    }

    private static DateOnly ObtenerFechaNacimiento(RegistroJugadorRequest request)
    {
        return request.FechaNacimiento
            ?? throw new DatosInvalidosException("La fecha de nacimiento es obligatoria.");
    }

    private UsuarioResponse CrearUsuario(RegistroJugadorRequest request, Rol rol, DateOnly fechaNacimiento)
    {
        ValidarEmailDisponible(request.Email);
        ValidarNicknameDisponible(request.Nickname);
        var fechaActual = ObtenerFechaActual();
        var contrasenaHash = hasheadorContrasenas.Hashear(request.Contrasena);
        var usuario = new Usuario(
            request.Nombre,
            request.Apellido,
            request.Email,
            request.Nickname,
            fechaNacimiento,
            rol,
            contrasenaHash,
            fechaActual);
        repositorioUsuarios.Agregar(usuario);
        return CrearResponse(usuario);
    }

    private void ValidarEmailDisponible(string email)
    {
        if(repositorioUsuarios.ObtenerPorEmail(email) is not null)
        {
            throw new EmailYaExisteException();
        }
    }

    private void ValidarNicknameDisponible(string nickname)
    {
        if(repositorioUsuarios.ExistePorNickname(nickname))
        {
            throw new NicknameYaExisteException();
        }
    }

    private DateOnly ObtenerFechaActual()
    {
        return DateOnly.FromDateTime(reloj.GetLocalNow().DateTime);
    }

    private static Rol ConvertirRol(string rol)
    {
        if(!Enum.TryParse<Rol>(rol, out var rolConvertido) || !Enum.IsDefined(rolConvertido))
        {
            throw new DatosInvalidosException("El rol debe ser Jugador, Organizador o Administrador.");
        }

        return rolConvertido;
    }

    // Los índices únicos de SQL Server no distinguen mayúsculas: cambiar solo mayúsculas no ocupa un valor ajeno.
    private static bool MismoTexto(string actual, string nuevo)
    {
        return string.Equals(actual, nuevo, StringComparison.OrdinalIgnoreCase);
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
