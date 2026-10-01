namespace FarmeORT.DTOs.Skills;

public sealed class FiltroSkillsRequest
{
    public string? Categoria { get; init; }

    public int? Dificultad { get; init; }

    public bool? Activa { get; init; }

    public string? Nombre { get; init; }

    public int Limit { get; init; }

    public int Offset { get; init; }
}
