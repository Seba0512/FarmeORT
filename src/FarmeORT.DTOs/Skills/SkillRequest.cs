using System.ComponentModel.DataAnnotations;

namespace FarmeORT.DTOs.Skills;

public sealed class SkillRequest
{
    [Required(ErrorMessage = "El código es obligatorio.")]
    [RegularExpression("^[a-zA-Z0-9]{5,20}$", ErrorMessage = "El código debe ser alfanumérico, sin espacios, de 5 a 20 caracteres.")]
    public string Codigo { get; init; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(40, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 40 caracteres.")]
    public string Nombre { get; init; } = string.Empty;

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(300, MinimumLength = 20, ErrorMessage = "La descripción debe tener entre 20 y 300 caracteres.")]
    public string Descripcion { get; init; } = string.Empty;

    [Required(ErrorMessage = "La categoría es obligatoria.")]
    [AllowedValues(
        CategoriasSkill.Gestual,
        CategoriasSkill.Vocal,
        CategoriasSkill.Baile,
        CategoriasSkill.Meme,
        ErrorMessage = "La categoría debe ser Gestual, Vocal, Baile o Meme.")]
    public string Categoria { get; init; } = string.Empty;

    [Required(ErrorMessage = "La dificultad es obligatoria.")]
    [Range(1, 5, ErrorMessage = "La dificultad debe ser un entero entre 1 y 5.")]
    public int? Dificultad { get; init; }
}
