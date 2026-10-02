using FarmeORT.Dominio.Inscripciones;
using FarmeORT.Excepciones;
using FarmeORT.IRepositorios.Inscripciones;
using Microsoft.EntityFrameworkCore;

namespace FarmeORT.Repositorios.Inscripciones;

internal sealed class RepositorioInscripciones(FarmeORTContext contexto) : IRepositorioInscripciones
{
    public Inscripcion? ObtenerPorId(int id)
    {
        return contexto.Inscripciones
            .Include(i => i.Torneo)
            .Include(i => i.Jugador)
            .SingleOrDefault(i => i.Id == id);
    }

    public IReadOnlyList<Inscripcion> Listar(FiltroInscripciones filtro)
    {
        var consulta = contexto.Inscripciones.Include(i => i.Torneo).Include(i => i.Jugador).AsQueryable();
        if(filtro.TorneoId != null)
        {
            consulta = consulta.Where(i => i.TorneoId == filtro.TorneoId.Value);
        }

        if(filtro.Estado != null)
        {
            consulta = consulta.Where(i => i.Estado == filtro.Estado.Value);
        }

        if(filtro.JugadorId != null)
        {
            consulta = consulta.Where(i => i.JugadorId == filtro.JugadorId.Value);
        }

        if(filtro.OrganizadorId != null)
        {
            consulta = consulta.Where(i => i.Torneo.OrganizadorId == filtro.OrganizadorId.Value);
        }

        // Las fechas del filtro son días completos: hasta incluye todo ese día.
        if(filtro.FechaDesde != null)
        {
            var desde = filtro.FechaDesde.Value.ToDateTime(TimeOnly.MinValue);
            consulta = consulta.Where(i => i.Fecha >= desde);
        }

        if(filtro.FechaHasta != null)
        {
            var hasta = filtro.FechaHasta.Value.AddDays(1).ToDateTime(TimeOnly.MinValue);
            consulta = consulta.Where(i => i.Fecha < hasta);
        }

        return consulta
            .OrderBy(i => i.Id)
            .Skip(filtro.Desplazamiento)
            .Take(filtro.Limite)
            .ToList();
    }

    public bool ExisteAceptada(int torneoId, int jugadorId)
    {
        return contexto.Inscripciones.Any(i =>
            i.TorneoId == torneoId && i.JugadorId == jugadorId && i.Estado == EstadoInscripcion.Aceptada);
    }

    public int ContarAceptadas(int torneoId)
    {
        return contexto.Inscripciones.Count(i => i.TorneoId == torneoId && i.Estado == EstadoInscripcion.Aceptada);
    }

    public IReadOnlyDictionary<int, int> ContarAceptadasPorTorneo(IReadOnlyCollection<int> torneoIds)
    {
        return contexto.Inscripciones
            .Where(i => torneoIds.Contains(i.TorneoId) && i.Estado == EstadoInscripcion.Aceptada)
            .GroupBy(i => i.TorneoId)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public IReadOnlyList<Inscripcion> ListarAceptadasDelTorneo(int torneoId)
    {
        return contexto.Inscripciones
            .Include(i => i.Jugador)
            .ThenInclude(j => j.Repertorio)
            .Where(i => i.TorneoId == torneoId && i.Estado == EstadoInscripcion.Aceptada)
            .OrderBy(i => i.Id)
            .ToList();
    }

    public IReadOnlyList<Inscripcion> ListarAceptadasDelJugador(int jugadorId)
    {
        return contexto.Inscripciones
            .Include(i => i.Torneo)
            .ThenInclude(t => t.SkillsObligatorias)
            .Where(i => i.JugadorId == jugadorId && i.Estado == EstadoInscripcion.Aceptada)
            .OrderBy(i => i.Id)
            .ToList();
    }

    public void Agregar(Inscripcion inscripcion)
    {
        contexto.Inscripciones.Add(inscripcion);
        try
        {
            contexto.SaveChanges();
        }
        catch(DbUpdateConcurrencyException)
        {
            contexto.ChangeTracker.Clear();
            if(ExisteAceptada(inscripcion.TorneoId, inscripcion.JugadorId))
            {
                throw new InscripcionYaExisteException();
            }

            var cupo = contexto.Torneos.Where(t => t.Id == inscripcion.TorneoId).Select(t => t.Cupo).Single();
            if(ContarAceptadas(inscripcion.TorneoId) >= cupo)
            {
                throw new TorneoCompletoException();
            }

            throw new TorneoModificadoException();
        }
    }

    public Inscripcion? ObtenerAceptada(int torneoId, int jugadorId)
    {
        return contexto.Inscripciones.SingleOrDefault(i =>
            i.TorneoId == torneoId && i.JugadorId == jugadorId && i.Estado == EstadoInscripcion.Aceptada);
    }

    public bool ExisteDelJugador(int torneoId, int jugadorId)
    {
        return contexto.Inscripciones.Any(i => i.TorneoId == torneoId && i.JugadorId == jugadorId);
    }

    public void Actualizar(Inscripcion inscripcion)
    {
        contexto.Inscripciones.Update(inscripcion);
        contexto.SaveChanges();
    }
}
