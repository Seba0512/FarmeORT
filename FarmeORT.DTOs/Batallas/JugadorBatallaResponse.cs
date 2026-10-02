namespace FarmeORT.DTOs.Batallas;

public sealed class JugadorBatallaResponse
{
    public int Id { get; init; }

    public string Nickname { get; init; } = string.Empty;

    public int Elo { get; init; }
}
