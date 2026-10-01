namespace FarmeORT.DTOs.Skills;

public class SkillResponse
{
    public int Id { get; init; }

    public string Codigo { get; init; } = string.Empty;

    public string Nombre { get; init; } = string.Empty;

    public string Descripcion { get; init; } = string.Empty;

    public string Categoria { get; init; } = string.Empty;

    public int Dificultad { get; init; }

    public bool Activa { get; init; }
}
