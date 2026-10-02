using FarmeORT.DTOs.Skills;

namespace FarmeORT.DTOs.Torneos;

// El cuadro (rondas y batallas) se agrega con las rondas.
public sealed class TorneoDetalleResponse : TorneoResponse
{
    public IReadOnlyList<SkillResponse> SkillsObligatorias { get; init; } = [];
}
