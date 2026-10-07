using FarmeORT.Dominio.Batallas;

namespace FarmeORT.IRepositorios.Batallas;

public sealed record FiltroBatallas(
    int TorneoId,
    EstadoBatalla? Estado,
    int? Ronda,
    int? JugadorId,
    int Limite,
    int Desplazamiento);
