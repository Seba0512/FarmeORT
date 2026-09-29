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
        if(usuario is null || !hasheadorContrasenas.Verificar(request.Contrasena, usuario.ContrasenaHash))
        {
            throw new CredencialesInvalidasException();
        }

        _ = repositorioSesiones;
        _ = generadorTokens;
        throw new NotImplementedException();
    }
}
