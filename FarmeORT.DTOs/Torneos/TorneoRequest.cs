namespace FarmeORT.DTOs.Torneos;

public sealed class TorneoRequest
{
    public string Codigo { get; init; } = string.Empty;

    public string Nombre { get; init; } = string.Empty;

    public string Descripcion { get; init; } = string.Empty;

    public DateOnly? FechaInicio { get; init; }

    public DateOnly? FechaFin { get; init; }

    public int? Cupo { get; init; }

    public int? OrganizadorId { get; init; }
}
