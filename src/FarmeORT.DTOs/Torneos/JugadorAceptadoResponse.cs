namespace FarmeORT.DTOs.Torneos;

public sealed class JugadorAceptadoResponse
{
    public int JugadorId { get; init; }

    public string Nickname { get; init; } = string.Empty;

    public int Elo { get; init; }
}
