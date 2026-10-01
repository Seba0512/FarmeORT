using FarmeORT.Dominio.Batallas;
using FarmeORT.IRepositorios.Batallas;
using Microsoft.EntityFrameworkCore;

namespace FarmeORT.Repositorios.Batallas;

internal sealed class RepositorioBatallas(FarmeORTContext contexto) : IRepositorioBatallas
{
    public Batalla? ObtenerPorId(int id)
    {
        return contexto.Batallas
            .Include(b => b.Torneo).ThenInclude(t => t.Organizador)
            .Include(b => b.Jugador1)
            .Include(b => b.Jugador2)
            .Include(b => b.Lanzamientos.OrderBy(l => l.Turno)).ThenInclude(l => l.Skill)
            .SingleOrDefault(b => b.Id == id);
    }

    public void Guardar()
    {
        throw new NotImplementedException();
    }
}
