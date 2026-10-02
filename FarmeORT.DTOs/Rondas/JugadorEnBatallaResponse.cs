namespace FarmeORT.DTOs.Rondas;

public sealed class JugadorEnBatallaResponse
{
    public int Id { get; init; }

    public string Nickname { get; init; } = string.Empty;

    public int Elo { get; init; }
}
