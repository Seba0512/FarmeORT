using FarmeORT.Dominio.Usuarios;

namespace FarmeORT.Dominio.Rondas;

public interface IEstrategiaEmparejamiento
{
    IReadOnlyList<ParDeJugadores> Emparejar(IReadOnlyList<Usuario> jugadores);
}
