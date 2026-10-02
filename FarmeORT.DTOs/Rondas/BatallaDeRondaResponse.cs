namespace FarmeORT.DTOs.Rondas;

public sealed class BatallaDeRondaResponse
{
    public int Id { get; init; }

    public string Estado { get; init; } = string.Empty;

    public JugadorEnBatallaResponse Jugador1 { get; init; } = new();

    public JugadorEnBatallaResponse Jugador2 { get; init; } = new();

    public int? GanadorId { get; init; }
}
