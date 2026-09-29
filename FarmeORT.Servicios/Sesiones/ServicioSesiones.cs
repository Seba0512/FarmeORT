using FarmeORT.DTOs.Sesiones;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Usuarios;
using FarmeORT.IServicios.Seguridad;
using FarmeORT.IServicios.Sesiones;

namespace FarmeORT.Servicios.Sesiones;

internal sealed class ServicioSesiones(
    IRepositorioUsuarios repositorioUsuarios,
    IHasheadorContrasenas hasheadorContrasenas) : IServicioSesiones
{
    public SesionResponse IniciarSesion(LoginRequest request)
    {
        var usuario = repositorioUsuarios.ObtenerPorEmail(request.Email);
        if(usuario is null)
        {
            throw new CredencialesInvalidasException();
        }

        _ = hasheadorContrasenas;
        throw new NotImplementedException();
    }
}
