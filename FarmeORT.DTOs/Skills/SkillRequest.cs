namespace FarmeORT.DTOs.Skills;

public sealed class SkillRequest
{
    public string Codigo { get; init; } = string.Empty;

    public string Nombre { get; init; } = string.Empty;

    public string Descripcion { get; init; } = string.Empty;

    public string Categoria { get; init; } = string.Empty;

    public int? Dificultad { get; init; }
}
