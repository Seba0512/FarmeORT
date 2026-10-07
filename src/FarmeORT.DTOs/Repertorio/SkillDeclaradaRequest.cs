using System.ComponentModel.DataAnnotations;

namespace FarmeORT.DTOs.Repertorio;

public sealed class SkillDeclaradaRequest
{
    [Required(ErrorMessage = "La skill es obligatoria.")]
    public int? SkillId { get; init; }

    [Required(ErrorMessage = "El nivel es obligatorio.")]
    [Range(1, 5, ErrorMessage = "El nivel debe ser un entero entre 1 y 5.")]
    public int? Nivel { get; init; }
}
