namespace FarmeORT.DTOs.Batallas;

public sealed class LanzamientoResponse
{
    public int Turno { get; init; }

    public int JugadorId { get; init; }

    public string Nickname { get; init; } = string.Empty;

    public int SkillId { get; init; }

    public string Skill { get; init; } = string.Empty;

    public DateTimeOffset Fecha { get; init; }
}
