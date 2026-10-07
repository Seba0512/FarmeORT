namespace FarmeORT.DTOs.Repertorio;

public sealed class SkillDeclaradaResponse
{
    public int SkillId { get; init; }

    public string Codigo { get; init; } = string.Empty;

    public string Nombre { get; init; } = string.Empty;

    public string Categoria { get; init; } = string.Empty;

    public bool Activa { get; init; }

    public int Nivel { get; init; }

    public DateOnly FechaDeclaracion { get; init; }
}
