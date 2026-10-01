using FarmeORT.Dominio.Inscripciones;
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
        _ = filtro;
        throw new NotImplementedException();
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
        _ = torneoIds;
        throw new NotImplementedException();
    }

    public IReadOnlyList<Inscripcion> ListarAceptadasDelTorneo(int torneoId)
    {
        _ = torneoId;
        throw new NotImplementedException();
    }

    public IReadOnlyList<Inscripcion> ListarAceptadasDelJugador(int jugadorId)
    {
        _ = jugadorId;
        throw new NotImplementedException();
    }

    public void Agregar(Inscripcion inscripcion)
    {
        contexto.Inscripciones.Add(inscripcion);
        contexto.SaveChanges();
    }

    public void Actualizar(Inscripcion inscripcion)
    {
        _ = inscripcion;
        throw new NotImplementedException();
    }
}
