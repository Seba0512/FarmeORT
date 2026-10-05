using FarmeORT.Dominio.Sesiones;
using FarmeORT.DTOs.Sesiones;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Sesiones;
using FarmeORT.IRepositorios.Usuarios;
using FarmeORT.IServicios.Seguridad;
using FarmeORT.IServicios.Sesiones;

namespace FarmeORT.Servicios.Sesiones;

internal sealed class ServicioSesiones(
    IRepositorioUsuarios repositorioUsuarios,
    IRepositorioSesiones repositorioSesiones,
    IHasheadorContrasenas hasheadorContrasenas,
    IGeneradorTokens generadorTokens) : IServicioSesiones
{
    public SesionResponse IniciarSesion(LoginRequest request)
    {
        var usuario = repositorioUsuarios.ObtenerPorEmail(request.Email);
        if(usuario == null || usuario.Eliminado || !hasheadorContrasenas.Verificar(request.Contrasena, usuario.ContrasenaHash))
        {
            throw new CredencialesInvalidasException();
        }

        var token = generadorTokens.Generar();
        repositorioSesiones.Agregar(new Sesion(token, usuario));
        return new SesionResponse
        {
            Token = token.ToString(),
            UsuarioId = usuario.Id,
            Rol = usuario.Rol.ToString(),
        };
    }

    // El token identifica a la sesión; cada usuario solo cierra las suyas.
    public void CerrarSesion(Guid token, int usuarioId)
    {
        var sesion = repositorioSesiones.ObtenerPorToken(token);
        if(sesion == null)
        {
            throw new SesionNoEncontradaException();
        }

        if(sesion.UsuarioId != usuarioId)
        {
            throw new NoEsDuenoDeLaSesionException();
        }

        repositorioSesiones.Eliminar(token);
    }

    public SesionAutenticada? ObtenerSesionAutenticada(Guid token)
    {
        var sesion = repositorioSesiones.ObtenerPorToken(token);
        if(sesion == null)
        {
            return null;
        }

        return new SesionAutenticada
        {
            Token = sesion.Token,
            UsuarioId = sesion.UsuarioId,
            Rol = sesion.Usuario.Rol.ToString(),
        };
    }
}
