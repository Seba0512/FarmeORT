using FarmeORT.Dominio.Sesiones;

namespace FarmeORT.IRepositorios.Sesiones;

public interface IRepositorioSesiones
{
    void Agregar(Sesion sesion);

    void Eliminar(Guid token);

    Sesion? ObtenerPorToken(Guid token);
}
