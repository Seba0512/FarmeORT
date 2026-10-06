using System.Data;
using FarmeORT.Dominio.Sesiones;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Sesiones;
using Microsoft.EntityFrameworkCore;

namespace FarmeORT.Repositorios.Sesiones;

internal sealed class RepositorioSesiones(FarmeORTContext contexto) : IRepositorioSesiones
{
    public void Agregar(Sesion sesion)
    {
        using var transaccion = contexto.Database.BeginTransaction(IsolationLevel.Serializable);

        // La escritura condicionada coordina el alta con la baja sobre la misma fila, hasta el commit.
        var activos = contexto.Usuarios.Where(u => u.Id == sesion.UsuarioId && !u.Eliminado)
            .ExecuteUpdate(cambios => cambios.SetProperty(u => u.Eliminado, false));
        if(activos == 0)
        {
            throw new CredencialesInvalidasException();
        }

        contexto.Sesiones.Add(sesion);
        contexto.SaveChanges();
        transaccion.Commit();
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
