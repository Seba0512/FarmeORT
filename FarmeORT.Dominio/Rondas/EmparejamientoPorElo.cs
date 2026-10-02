using FarmeORT.Dominio.Usuarios;

namespace FarmeORT.Dominio.Rondas;

public sealed class EmparejamientoPorElo : IEstrategiaEmparejamiento
{
    public IReadOnlyList<ParDeJugadores> Emparejar(IReadOnlyList<Usuario> jugadores)
    {
        var ordenados = jugadores.OrderBy(j => j.Id).ToList();
        List<ParDeJugadores> pares = [];
        for(var i = 0; i < ordenados.Count / 2; i++)
        {
            pares.Add(new ParDeJugadores(ordenados[i], ordenados[ordenados.Count - 1 - i]));
        }

        return pares;
    }
}
