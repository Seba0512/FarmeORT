using FarmeORT.Dominio.Sesiones;
using FarmeORT.IRepositorios.Sesiones;

namespace FarmeORT.Repositorios.Sesiones;

internal sealed class RepositorioSesiones(FarmeORTContext contexto) : IRepositorioSesiones
{
    public void Agregar(Sesion sesion)
    {
        _ = contexto;
        throw new NotImplementedException();
    }
}
