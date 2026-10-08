namespace FarmeORT.DTOs.Ranking;

public sealed class FiltroRankingRequest
{
    public const int CantidadPorDefecto = 10;

    public int Cantidad { get; init; } = CantidadPorDefecto;

    public int? TorneoId { get; init; }
}
