using FarmeORT.Dominio.Batallas;
using FarmeORT.IRepositorios.Batallas;

namespace FarmeORT.Repositorios.Batallas;

internal sealed class RepositorioBatallas(FarmeORTContext contexto) : IRepositorioBatallas
{
    public Batalla? ObtenerPorId(int id)
    {
        throw new NotImplementedException(contexto.GetType().Name);
    }

    public void Guardar()
    {
        throw new NotImplementedException();
    }
}
