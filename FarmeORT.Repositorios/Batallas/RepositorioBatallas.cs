using FarmeORT.Dominio.Batallas;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Batallas;
using Microsoft.EntityFrameworkCore;

namespace FarmeORT.Repositorios.Batallas;

internal sealed class RepositorioBatallas(FarmeORTContext contexto) : IRepositorioBatallas
{
    public Batalla? ObtenerPorId(int id)
    {
        return contexto.Batallas
            .Include(b => b.Torneo).ThenInclude(t => t.Organizador)
            .Include(b => b.Torneo).ThenInclude(t => t.SkillsObligatorias)
            .Include(b => b.Jugador1)
            .Include(b => b.Jugador2)
            .Include(b => b.Lanzamientos.OrderBy(l => l.Turno)).ThenInclude(l => l.Skill)
            .SingleOrDefault(b => b.Id == id);
    }

    public void Guardar()
    {
        try
        {
            contexto.SaveChanges();
        }
        catch(DbUpdateConcurrencyException)
        {
            contexto.ChangeTracker.Clear();
            throw new BatallaModificadaException();
        }
        catch(DbUpdateException)
        {
            var nuevos = contexto.ChangeTracker.Entries<Lanzamiento>()
                .Where(e => e.State == EntityState.Added)
                .Select(e => e.Entity)
                .ToList();
            if(nuevos.Any(l => contexto.Lanzamientos.AsNoTracking()
                .Any(guardado => guardado.BatallaId == l.BatallaId && guardado.Turno == l.Turno)))
            {
                contexto.ChangeTracker.Clear();
                throw new TurnoIncorrectoException();
            }

            if(nuevos.Any(l => contexto.Lanzamientos.AsNoTracking()
                .Any(guardado => guardado.BatallaId == l.BatallaId
                    && guardado.JugadorId == l.JugadorId && guardado.SkillId == l.SkillId)))
            {
                contexto.ChangeTracker.Clear();
                throw new SkillYaLanzadaException();
            }

            throw;
        }
    }

    public IReadOnlyList<Batalla> ListarSinResolverDelTorneo(int torneoId)
    {
        return contexto.Batallas
            .Where(b => b.TorneoId == torneoId
                && (b.Estado == EstadoBatalla.Pendiente || b.Estado == EstadoBatalla.EnArbitraje))
            .OrderBy(b => b.Id)
            .ToList();
    }

    public bool ExisteSinResolverDelJugador(int jugadorId)
    {
        return contexto.Batallas.Any(b => (b.Jugador1Id == jugadorId || b.Jugador2Id == jugadorId)
            && (b.Estado == EstadoBatalla.Pendiente || b.Estado == EstadoBatalla.EnArbitraje));
    }

    // No guarda: la batalla se persiste con el SaveChanges que guarda el torneo, en la misma transacción (D-62).
    public void Agregar(Batalla batalla)
    {
        contexto.Batallas.Add(batalla);
    }

    public IReadOnlyList<Batalla> ListarDelTorneo(int torneoId)
    {
        return contexto.Batallas
            .Include(b => b.Jugador1)
            .Include(b => b.Jugador2)
            .Where(b => b.TorneoId == torneoId)
            .OrderBy(b => b.NumeroRonda)
            .ThenBy(b => b.Id)
            .ToList();
    }
}
