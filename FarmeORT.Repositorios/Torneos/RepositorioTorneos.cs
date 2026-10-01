using FarmeORT.Dominio.Torneos;
using FarmeORT.IRepositorios.Torneos;
using Microsoft.EntityFrameworkCore;

namespace FarmeORT.Repositorios.Torneos;

internal sealed class RepositorioTorneos(FarmeORTContext contexto) : IRepositorioTorneos
{
    // Se incluye el organizador porque la respuesta muestra su nombre.
    public Torneo? ObtenerPorId(int id)
    {
        return contexto.Torneos
            .Include(t => t.Organizador)
            .SingleOrDefault(t => t.Id == id);
    }

    public IReadOnlyList<Torneo> Listar(FiltroTorneos filtro)
    {
        _ = filtro;
        throw new NotImplementedException();
    }

    public bool ExistePorCodigo(string codigo)
    {
        _ = codigo;
        throw new NotImplementedException();
    }

    public void Agregar(Torneo torneo)
    {
        contexto.Torneos.Add(torneo);
        contexto.SaveChanges();
    }

    public void Actualizar(Torneo torneo)
    {
        _ = torneo;
        throw new NotImplementedException();
    }
}
