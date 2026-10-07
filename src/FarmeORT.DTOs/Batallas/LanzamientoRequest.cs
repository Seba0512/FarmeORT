using System.ComponentModel.DataAnnotations;

namespace FarmeORT.DTOs.Batallas;

public sealed class LanzamientoRequest
{
    [Required(ErrorMessage = "La skill es obligatoria.")]
    [Range(1, int.MaxValue, ErrorMessage = "El id de la skill debe ser positivo.")]
    public int? SkillId { get; init; }
}
