using System.ComponentModel.DataAnnotations;

namespace FarmeORT.DTOs.Batallas;

public sealed class FalloRequest
{
    [Required(ErrorMessage = "El ganador es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El id del ganador debe ser positivo.")]
    public int? GanadorId { get; init; }

    [Required(ErrorMessage = "El comentario es obligatorio.")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "El comentario debe tener entre 10 y 500 caracteres.")]
    public string? Comentario { get; init; }
}
