namespace FarmeORT.DTOs.Ranking;

public sealed class JugadorRankingResponse
{
    public int Posicion { get; init; }

    public string Nickname { get; init; } = string.Empty;

    public int Elo { get; init; }

    public int BatallasGanadas { get; init; }

    public int BatallasPerdidas { get; init; }
}
