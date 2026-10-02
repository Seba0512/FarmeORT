using System.ComponentModel.DataAnnotations;

namespace FarmeORT.DTOs.Batallas;

public sealed class FiltroBatallasRequest
{
    [Required(ErrorMessage = "El torneo es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El id del torneo debe ser positivo.")]
    public int? TorneoId { get; init; }

    public string? Estado { get; init; }

    public int? Ronda { get; init; }

    public int? JugadorId { get; init; }

    public int Limit { get; init; } = 20;

    public int Offset { get; init; }
}
