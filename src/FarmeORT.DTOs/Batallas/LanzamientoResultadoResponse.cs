namespace FarmeORT.DTOs.Batallas;

public sealed class LanzamientoResultadoResponse
{
    public int Turno { get; init; }

    public string Estado { get; init; } = string.Empty;

    public int? JugadorTurnoId { get; init; }
}
