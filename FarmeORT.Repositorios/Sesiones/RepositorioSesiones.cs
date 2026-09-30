using FarmeORT.Dominio.Sesiones;
using FarmeORT.IRepositorios.Sesiones;
using Microsoft.EntityFrameworkCore;

namespace FarmeORT.Repositorios.Sesiones;

internal sealed class RepositorioSesiones(FarmeORTContext contexto) : IRepositorioSesiones
{
    public void Agregar(Sesion sesion)
    {
        contexto.Sesiones.Add(sesion);
        contexto.SaveChanges();
    }

    public void Eliminar(Guid token)
    {
        contexto.Sesiones.Where(s => s.Token == token).ExecuteDelete();
    }

    public Sesion? ObtenerPorToken(Guid token)
    {
        return contexto.Sesiones
            .Include(s => s.Usuario)
            .SingleOrDefault(s => s.Token == token);
    }
}
