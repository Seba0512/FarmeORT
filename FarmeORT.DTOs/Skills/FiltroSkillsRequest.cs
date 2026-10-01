namespace FarmeORT.DTOs.Skills;

// Sin validaciones de rango: el servicio usa el tope o el valor por defecto de la paginación e ignora los
// filtros con valores inválidos, "para no trancar al usuario" (AGENTS.md §7).
public sealed class FiltroSkillsRequest
{
    public string? Categoria { get; init; }

    public int? Dificultad { get; init; }

    public bool? Activa { get; init; }

    public string? Nombre { get; init; }

    public int Limit { get; init; } = 20;

    public int Offset { get; init; }
}
