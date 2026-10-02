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

    public IReadOnlyList<Batalla> Listar(FiltroBatallas filtro)
    {
        var consulta = contexto.Batallas.AsNoTracking()
            .Include(b => b.Jugador1)
            .Include(b => b.Jugador2)
            .Where(b => b.TorneoId == filtro.TorneoId);
        if(filtro.Estado != null)
        {
            consulta = consulta.Where(b => b.Estado == filtro.Estado);
        }

        if(filtro.Ronda != null)
        {
            consulta = consulta.Where(b => b.NumeroRonda == filtro.Ronda);
        }

        if(filtro.JugadorId != null)
        {
            consulta = consulta.Where(b => b.Jugador1Id == filtro.JugadorId || b.Jugador2Id == filtro.JugadorId);
        }

        return consulta.OrderBy(b => b.NumeroRonda).ThenBy(b => b.Id)
            .Skip(filtro.Desplazamiento).Take(filtro.Limite).ToList();
    }

    public IReadOnlyDictionary<int, ResultadosDeBatallas> ContarResultadosPorJugador(IReadOnlyCollection<int> jugadorIds, int? torneoId)
    {
        var finalizadas = contexto.Batallas.Where(b => b.Estado == EstadoBatalla.Finalizada && b.GanadorId != null);
        if(torneoId != null)
        {
            finalizadas = finalizadas.Where(b => b.TorneoId == torneoId.Value);
        }

        var ganadas = finalizadas
            .Where(b => jugadorIds.Contains(b.GanadorId!.Value))
            .GroupBy(b => b.GanadorId!.Value)
            .ToDictionary(g => g.Key, g => g.Count());
        var perdidasComoJugador1 = finalizadas
            .Where(b => jugadorIds.Contains(b.Jugador1Id) && b.GanadorId != b.Jugador1Id)
            .GroupBy(b => b.Jugador1Id)
            .ToDictionary(g => g.Key, g => g.Count());
        var perdidasComoJugador2 = finalizadas
            .Where(b => jugadorIds.Contains(b.Jugador2Id) && b.GanadorId != b.Jugador2Id)
            .GroupBy(b => b.Jugador2Id)
            .ToDictionary(g => g.Key, g => g.Count());
        return ganadas.Keys.Union(perdidasComoJugador1.Keys).Union(perdidasComoJugador2.Keys).ToDictionary(
            id => id,
            id => new ResultadosDeBatallas(
                ganadas.GetValueOrDefault(id),
                perdidasComoJugador1.GetValueOrDefault(id) + perdidasComoJugador2.GetValueOrDefault(id)));
    }
}
