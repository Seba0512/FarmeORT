using FarmeORT.DTOs.Skills;

namespace FarmeORT.DTOs.Torneos;

// El cuadro (rondas y batallas) se agrega con las rondas. Las propiedades propias del detalle tienen set: el
// servicio las completa después del mapeo que comparte con el resumen del torneo.
public sealed class TorneoDetalleResponse : TorneoResponse
{
    public IReadOnlyList<SkillResponse> SkillsObligatorias { get; set; } = [];

    public IReadOnlyList<JugadorAceptadoResponse> JugadoresAceptados { get; set; } = [];
}
