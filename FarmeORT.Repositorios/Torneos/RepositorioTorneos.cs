using FarmeORT.Dominio.Torneos;
using FarmeORT.IRepositorios.Torneos;

namespace FarmeORT.Repositorios.Torneos;

internal sealed class RepositorioTorneos(FarmeORTContext contexto) : IRepositorioTorneos
{
    public Torneo? ObtenerPorId(int id)
    {
        _ = contexto;
        _ = id;
        throw new NotImplementedException();
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
        _ = torneo;
        throw new NotImplementedException();
    }

    public void Actualizar(Torneo torneo)
    {
        _ = torneo;
        throw new NotImplementedException();
    }
}
