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
        return contexto.Torneos
            .Include(t => t.Organizador)
            .OrderBy(t => t.Id)
            .Skip(filtro.Desplazamiento)
            .Take(filtro.Limite)
            .ToList();
    }

    // SQL Server no distingue mayúsculas y SQLite sí: se compara en minúscula para responder igual en los dos (D-31).
    public bool ExistePorCodigo(string codigo)
    {
        var codigoBuscado = codigo.ToLower();
        return contexto.Torneos.Any(t => t.Codigo.ToLower() == codigoBuscado);
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
