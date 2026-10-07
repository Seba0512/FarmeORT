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

    public IReadOnlyDictionary<int, ResultadosDeBatallas> ContarResultadosPorJugador(IReadOnlyCollection<int> jugadorIds)
    {
        // Una sola consulta trae las batallas Finalizadas de los jugadores pedidos y se cuentan en memoria: agrupar
        // por el perdedor dentro de la consulta dio un conteo incorrecto en los tests con SQLite (D-46).
        var finalizadas = contexto.Batallas.AsNoTracking()
            .Where(b => b.Estado == EstadoBatalla.Finalizada && b.GanadorId != null)
            .Where(b => jugadorIds.Contains(b.Jugador1Id) || jugadorIds.Contains(b.Jugador2Id))
            .ToList();
        Dictionary<int, ResultadosDeBatallas> resultados = [];
        foreach(var jugadorId in jugadorIds)
        {
            var jugadas = finalizadas.Count(b => b.Jugador1Id == jugadorId || b.Jugador2Id == jugadorId);
            var ganadas = finalizadas.Count(b => b.GanadorId == jugadorId);
            if(jugadas > 0)
            {
                resultados[jugadorId] = new ResultadosDeBatallas(ganadas, jugadas - ganadas);
            }
        }

        return resultados;
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
        try
        {
            contexto.SaveChanges();
        }
        catch(DbUpdateConcurrencyException)
        {
            contexto.ChangeTracker.Clear();
            if(contexto.Batallas.Any(b => b.TorneoId == torneo.Id
                && (b.Estado == EstadoBatalla.Pendiente || b.Estado == EstadoBatalla.EnArbitraje)))
            {
                throw new RondaSinResolverException();
            }

            throw new TorneoModificadoException();
        }
    }
}
