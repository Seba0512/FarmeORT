using System.ComponentModel.DataAnnotations;

namespace FarmeORT.DTOs.Skills;

public sealed class SkillEstadoRequest
{
    [Required(ErrorMessage = "El estado activa es obligatorio.")]
    public bool? Activa { get; init; }
}
