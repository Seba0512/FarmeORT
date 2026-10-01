using System.ComponentModel.DataAnnotations;

namespace FarmeORT.DTOs.Skills;

public class FiltroSkillsRequest
{
    // null está permitido porque el filtro es opcional: sin categoría se listan todas.
    [AllowedValues(
        null,
        CategoriasSkill.Gestual,
        CategoriasSkill.Vocal,
        CategoriasSkill.Baile,
        CategoriasSkill.Meme,
        ErrorMessage = "La categoría debe ser Gestual, Vocal, Baile o Meme.")]
    public string? Categoria { get; init; }

    [Range(1, 5, ErrorMessage = "La dificultad debe ser un entero entre 1 y 5.")]
    public int? Dificultad { get; init; }

    public bool? Activa { get; init; }

    public string? Nombre { get; init; }

    [Range(1, 100, ErrorMessage = "El límite debe estar entre 1 y 100.")]
    public int Limit { get; init; } = 20;

    [Range(0, int.MaxValue, ErrorMessage = "El desplazamiento no puede ser negativo.")]
    public int Offset { get; init; }
}
