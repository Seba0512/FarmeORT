using System.ComponentModel.DataAnnotations;

namespace FarmeORT.DTOs.Torneos;

public sealed class SkillObligatoriaRequest
{
    [Required(ErrorMessage = "La skill es obligatoria.")]
    public int? SkillId { get; init; }
}
