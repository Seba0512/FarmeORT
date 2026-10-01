using FarmeORT.Dominio.Batallas;
using FarmeORT.IRepositorios.Batallas;

namespace FarmeORT.Repositorios.Batallas;

internal sealed class RepositorioBatallas(FarmeORTContext contexto) : IRepositorioBatallas
{
    public Batalla? ObtenerPorId(int id)
    {
        return contexto.Batallas.SingleOrDefault(b => b.Id == id);
    }

    public void Guardar()
    {
        throw new NotImplementedException();
    }
}
