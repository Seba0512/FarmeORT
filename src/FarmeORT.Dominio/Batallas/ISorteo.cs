using FarmeORT.Dominio.Usuarios;

namespace FarmeORT.Dominio.Batallas;

public interface ISorteo
{
    Usuario ElegirEntre(Usuario jugador1, Usuario jugador2);
}
