namespace FarmeORT.DTOs.Ranking;

public sealed class FiltroRankingRequest
{
    public int Cantidad { get; init; } = 10;

    public int? TorneoId { get; init; }
}
