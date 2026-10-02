namespace FarmeORT.DTOs.Batallas;

public sealed class FiltroBatallasRequest
{
    public int? TorneoId { get; init; }

    public string? Estado { get; init; }

    public int? Ronda { get; init; }

    public int? JugadorId { get; init; }

    public int Limit { get; init; } = 20;

    public int Offset { get; init; }
}
