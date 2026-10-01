using System.ComponentModel.DataAnnotations;

namespace FarmeORT.DTOs.Torneos;

public sealed class TorneoRequest
{
    [Required(ErrorMessage = "El código es obligatorio.")]
    [RegularExpression("^[a-zA-Z0-9]{5,20}$", ErrorMessage = "El código debe ser alfanumérico, sin espacios, de 5 a 20 caracteres.")]
    public string Codigo { get; init; } = string.Empty;

    public string Nombre { get; init; } = string.Empty;

    public string Descripcion { get; init; } = string.Empty;

    public DateOnly? FechaInicio { get; init; }

    public DateOnly? FechaFin { get; init; }

    public int? Cupo { get; init; }

    public int? OrganizadorId { get; init; }
}
