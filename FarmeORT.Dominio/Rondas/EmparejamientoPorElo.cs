using FarmeORT.Dominio.Usuarios;

namespace FarmeORT.Dominio.Rondas;

public sealed class EmparejamientoPorElo : IEstrategiaEmparejamiento
{
    public IReadOnlyList<ParDeJugadores> Emparejar(IReadOnlyList<Usuario> jugadores)
    {
        _ = jugadores;
        throw new NotImplementedException();
    }
}
