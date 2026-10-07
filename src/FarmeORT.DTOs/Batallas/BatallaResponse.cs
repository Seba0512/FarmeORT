namespace FarmeORT.DTOs.Batallas;

public sealed class BatallaResponse
{
    public int Id { get; init; }

    public int TorneoId { get; init; }

    public int Ronda { get; init; }

    public JugadorBatallaResponse Jugador1 { get; init; } = new();

    public JugadorBatallaResponse Jugador2 { get; init; } = new();

    public string Estado { get; init; } = string.Empty;

    public int? JugadorTurnoId { get; init; }

    public int? GanadorId { get; init; }
}
