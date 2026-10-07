namespace FarmeORT.DTOs.Torneos;

public sealed class FiltroTorneosRequest
{
    public string? Estado { get; init; }

    public DateOnly? FechaDesde { get; init; }

    public DateOnly? FechaHasta { get; init; }

    public string? Nombre { get; init; }

    public int? SkillObligatoriaId { get; init; }

    public int Limit { get; init; } = 20;

    public int Offset { get; init; }
}
