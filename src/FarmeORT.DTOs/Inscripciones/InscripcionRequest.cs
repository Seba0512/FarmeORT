using System.ComponentModel.DataAnnotations;

namespace FarmeORT.DTOs.Inscripciones;

public sealed class InscripcionRequest
{
    [Required(ErrorMessage = "El torneo es obligatorio.")]
    public int? TorneoId { get; init; }
}
