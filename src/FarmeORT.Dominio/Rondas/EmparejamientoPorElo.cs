using FarmeORT.Dominio.Usuarios;

namespace FarmeORT.Dominio.Rondas;

// Mejor contra peor: de mayor a menor ELO y, si empatan, por id (nunca el orden en que llegan), el primero enfrenta
// al último, el segundo al penúltimo, y así (D-62).
public sealed class EmparejamientoPorElo : IEstrategiaEmparejamiento
{
    public IReadOnlyList<ParDeJugadores> Emparejar(IReadOnlyList<Usuario> jugadores)
    {
        // Con el cupo potencia de 2 y un eliminado por batalla siempre llega una cantidad par. Si no, un jugador
        // quedaría afuera sin aviso: es un error de datos, no del cliente, y responde 500 (D-62).
        if(jugadores.Count % 2 != 0)
        {
            throw new InvalidOperationException("No se puede emparejar una cantidad impar de jugadores.");
        }

        var ordenados = jugadores.OrderByDescending(j => j.Elo).ThenBy(j => j.Id).ToList();
        List<ParDeJugadores> pares = [];
        for(var i = 0; i < ordenados.Count / 2; i++)
        {
            pares.Add(new ParDeJugadores(ordenados[i], ordenados[ordenados.Count - 1 - i]));
        }

        return pares;
    }
}
