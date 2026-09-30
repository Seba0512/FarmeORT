using FarmeORT.Dominio.Sesiones;
using FarmeORT.IRepositorios.Sesiones;

namespace FarmeORT.Repositorios.Sesiones;

internal sealed class RepositorioSesiones(FarmeORTContext contexto) : IRepositorioSesiones
{
    public void Agregar(Sesion sesion)
    {
        contexto.Sesiones.Add(sesion);
        contexto.SaveChanges();
    }
}
