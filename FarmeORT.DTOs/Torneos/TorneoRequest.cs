using System.ComponentModel.DataAnnotations;

namespace FarmeORT.DTOs.Torneos;

public sealed class TorneoRequest
{
    [Required(ErrorMessage = "El código es obligatorio.")]
    [RegularExpression("^[a-zA-Z0-9]{5,20}$", ErrorMessage = "El código debe ser alfanumérico, sin espacios, de 5 a 20 caracteres.")]
    public string Codigo { get; init; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(60, MinimumLength = 5, ErrorMessage = "El nombre debe tener entre 5 y 60 caracteres.")]
    public string Nombre { get; init; } = string.Empty;

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(500, MinimumLength = 20, ErrorMessage = "La descripción debe tener entre 20 y 500 caracteres.")]
    public string Descripcion { get; init; } = string.Empty;

    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    public DateOnly? FechaInicio { get; init; }

    [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
    public DateOnly? FechaFin { get; init; }

    [Required(ErrorMessage = "El cupo es obligatorio.")]
    [Range(4, 64, ErrorMessage = "El cupo debe estar entre 4 y 64.")]
    public int? Cupo { get; init; }

    [Required(ErrorMessage = "El organizador es obligatorio.")]
    public int? OrganizadorId { get; init; }
}
