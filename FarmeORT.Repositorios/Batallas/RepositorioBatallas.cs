using System.Linq.Expressions;
using FarmeORT.Dominio.Batallas;
using FarmeORT.Dominio.Torneos;
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

        var ganadas = ContarPorJugador(
            finalizadas.Where(b => jugadorIds.Contains(b.GanadorId!.Value)), b => b.GanadorId!.Value);

        // Las perdidas se cuentan por cada lado de la batalla: agrupar por el perdedor calculado con un CASE dio un
        // conteo incorrecto en los tests con SQLite (D-46).
        var perdidasComoJugador1 = ContarPorJugador(
            finalizadas.Where(b => jugadorIds.Contains(b.Jugador1Id) && b.GanadorId != b.Jugador1Id), b => b.Jugador1Id);
        var perdidasComoJugador2 = ContarPorJugador(
            finalizadas.Where(b => jugadorIds.Contains(b.Jugador2Id) && b.GanadorId != b.Jugador2Id), b => b.Jugador2Id);
        return ganadas.Keys.Union(perdidasComoJugador1.Keys).Union(perdidasComoJugador2.Keys).ToDictionary(
            id => id,
            id => new ResultadosDeBatallas(
                ganadas.GetValueOrDefault(id),
                perdidasComoJugador1.GetValueOrDefault(id) + perdidasComoJugador2.GetValueOrDefault(id)));
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

    // Un solo SaveChanges: las batallas de la ronda y el torneo En curso se guardan juntos o no se guarda nada (D-62).
    public void GuardarRonda(Torneo torneo, IReadOnlyList<Batalla> batallas)
    {
        contexto.Batallas.AddRange(batallas);
        contexto.Torneos.Update(torneo);
        contexto.SaveChanges();
    }

    private static Dictionary<int, int> ContarPorJugador(IQueryable<Batalla> batallas, Expression<Func<Batalla, int>> jugador)
    {
        return batallas.GroupBy(jugador).ToDictionary(g => g.Key, g => g.Count());
    }
}
