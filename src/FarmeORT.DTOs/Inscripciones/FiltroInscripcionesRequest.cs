namespace FarmeORT.DTOs.Inscripciones;

// Sin validaciones de rango: el servicio ajusta la paginación e ignora los filtros inválidos (D-35).
public sealed class FiltroInscripcionesRequest
{
    public int? TorneoId { get; init; }

    public string? Estado { get; init; }

    public DateOnly? FechaDesde { get; init; }

    public DateOnly? FechaHasta { get; init; }

    public int Limit { get; init; } = Paginado.LimitePorDefecto;

    public int Offset { get; init; }
}
