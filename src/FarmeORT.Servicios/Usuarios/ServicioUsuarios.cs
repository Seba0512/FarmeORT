using FarmeORT.Dominio.Usuarios;
using FarmeORT.DTOs.Usuarios;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Batallas;
using FarmeORT.IRepositorios.Inscripciones;
using FarmeORT.IRepositorios.Torneos;
using FarmeORT.IRepositorios.Usuarios;
using FarmeORT.IServicios.Seguridad;
using FarmeORT.IServicios.Usuarios;

namespace FarmeORT.Servicios.Usuarios;

internal sealed class ServicioUsuarios(
    IRepositorioUsuarios repositorioUsuarios,
    IRepositorioTorneos repositorioTorneos,
    IRepositorioBatallas repositorioBatallas,
    IRepositorioInscripciones repositorioInscripciones,
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
        var usuario = ObtenerUsuarioExistente(id);
        return CrearResponse(usuario);
    }

    public IReadOnlyList<UsuarioResponse> Listar(FiltroUsuariosRequest filtro)
    {
        var limite = Paginacion.AjustarLimite(filtro.Limit);
        var desplazamiento = Paginacion.AjustarDesplazamiento(filtro.Offset);

        var filtroRepositorio = new FiltroUsuarios(filtro.Nombre, filtro.Apellido, filtro.Nickname, limite, desplazamiento);
        var usuarios = repositorioUsuarios.Listar(filtroRepositorio);
        return usuarios.Select(CrearResponse).ToList();
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
        var usuario = ObtenerUsuarioExistente(id);
        ValidarCambios(usuario, request, rol);

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

        var usuario = ObtenerUsuarioExistente(id);
        ValidarSinParticipacionVigente(usuario);
        usuario.Eliminar();
        repositorioUsuarios.Eliminar(usuario);
    }

    // Solo se controla lo que cambia: mantener el propio email o nickname no choca consigo mismo.
    private void ValidarCambios(Usuario usuario, UsuarioAdminRequest request, Rol rol)
    {
        if(usuario.Rol != rol)
        {
            ValidarSinParticipacionVigente(usuario);
        }

        if(!MismoTexto(usuario.Email, request.Email))
        {
            ValidarEmailDisponible(request.Email);
        }

        if(!MismoTexto(usuario.Nickname, request.Nickname))
        {
            ValidarNicknameDisponible(request.Nickname);
        }
    }

    // Eliminar y cambiar el rol tienen las mismas restricciones (D-43): las dos primeras son de la letra y la
    // inscripción vigente evita que las rondas emparejen a alguien dado de baja o que ya no es Jugador.
    private void ValidarSinParticipacionVigente(Usuario usuario)
    {
        if(repositorioTorneos.ExisteVigenteDelOrganizador(usuario.Id))
        {
            throw new UsuarioOrganizaTorneoVigenteException();
        }

        if(repositorioBatallas.ExisteSinResolverDelJugador(usuario.Id))
        {
            throw new UsuarioConBatallasSinResolverException();
        }

        if(repositorioInscripciones.ListarAceptadasDelJugador(usuario.Id).Any(i => i.EstaVigente))
        {
            throw new UsuarioConInscripcionVigenteException();
        }
    }

    private Usuario ObtenerUsuarioExistente(int id)
    {
        var usuario = repositorioUsuarios.ObtenerPorId(id);
        if(usuario == null)
        {
            throw new UsuarioNoEncontradoException();
        }

        return usuario;
    }

    private static DateOnly ObtenerFechaNacimiento(RegistroJugadorRequest request)
    {
        return DatosObligatorios.Obtener(request.FechaNacimiento, "La fecha de nacimiento es obligatoria.");
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
        if(repositorioUsuarios.ObtenerPorEmail(email) != null)
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

    // Se compara por nombre para que un número como "1" no se acepte como rol.
    private static Rol ConvertirRol(string rol)
    {
        var rolConvertido = ValoresPorNombre.Buscar<Rol>(rol);
        if(rolConvertido == null)
        {
            throw new DatosInvalidosException("El rol debe ser Jugador, Organizador o Administrador.");
        }

        return rolConvertido.Value;
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
