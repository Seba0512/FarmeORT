using FarmeORT.Dominio.Batallas;

namespace FarmeORT.IRepositorios.Batallas;

public interface IRepositorioBatallas
{
    Batalla? ObtenerPorId(int id);

    void Guardar();

    // Batallas Pendientes o EnArbitraje en las que juega: impiden dar de baja al usuario o cambiarle el rol.
    bool ExisteSinResolverDelJugador(int jugadorId);
}
