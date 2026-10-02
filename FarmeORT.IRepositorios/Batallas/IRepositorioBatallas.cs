using FarmeORT.Dominio.Batallas;

namespace FarmeORT.IRepositorios.Batallas;

public interface IRepositorioBatallas
{
    Batalla? ObtenerPorId(int id);

    void Guardar();

    // Batallas Pendientes o EnArbitraje del torneo, rastreadas para cancelarlas en el mismo guardado que el torneo.
    IReadOnlyList<Batalla> ListarSinResolverDelTorneo(int torneoId);
}
