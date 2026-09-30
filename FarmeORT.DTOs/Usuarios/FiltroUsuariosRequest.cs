using System.ComponentModel.DataAnnotations;

namespace FarmeORT.DTOs.Usuarios;

public sealed class FiltroUsuariosRequest
{
    public string? Nombre { get; init; }

    public string? Apellido { get; init; }

    public string? Nickname { get; init; }

    [Range(1, 100, ErrorMessage = "El límite debe estar entre 1 y 100.")]
    public int Limit { get; init; } = 20;

    [Range(0, int.MaxValue, ErrorMessage = "El desplazamiento no puede ser negativo.")]
    public int Offset { get; init; }
}
