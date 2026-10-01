using FarmeORT.Dominio.Batallas;
using FarmeORT.Dominio.Usuarios;

namespace FarmeORT.Servicios.Batallas;

internal sealed class SorteoAleatorio : ISorteo
{
    public Usuario ElegirEntre(Usuario jugador1, Usuario jugador2)
    {
        return Random.Shared.Next(2) == 0 ? jugador1 : jugador2;
    }
}
