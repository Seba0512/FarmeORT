using FarmeORT.Dominio.Inscripciones;
using FarmeORT.IRepositorios.Inscripciones;

namespace FarmeORT.Repositorios.Inscripciones;

internal sealed class RepositorioInscripciones(FarmeORTContext contexto) : IRepositorioInscripciones
{
    public Inscripcion? ObtenerPorId(int id)
    {
        _ = contexto;
        _ = id;
        throw new NotImplementedException();
    }

    public IReadOnlyList<Inscripcion> Listar(FiltroInscripciones filtro)
    {
        _ = filtro;
        throw new NotImplementedException();
    }

    public bool ExisteAceptada(int torneoId, int jugadorId)
    {
        _ = torneoId;
        _ = jugadorId;
        throw new NotImplementedException();
    }

    public int ContarAceptadas(int torneoId)
    {
        _ = torneoId;
        throw new NotImplementedException();
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
        _ = inscripcion;
        throw new NotImplementedException();
    }

    public void Actualizar(Inscripcion inscripcion)
    {
        _ = inscripcion;
        throw new NotImplementedException();
    }
}
