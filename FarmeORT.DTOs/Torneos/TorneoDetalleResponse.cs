using FarmeORT.DTOs.Rondas;
using FarmeORT.DTOs.Skills;

namespace FarmeORT.DTOs.Torneos;

// Las propiedades propias del detalle tienen set: el servicio las completa después del mapeo que comparte con el
// resumen del torneo. Rondas es el cuadro: con una sola llamada alcanza para dibujarlo entero (D-62).
public sealed class TorneoDetalleResponse : TorneoResponse
{
    public IReadOnlyList<SkillResponse> SkillsObligatorias { get; set; } = [];

    public IReadOnlyList<JugadorAceptadoResponse> JugadoresAceptados { get; set; } = [];

    public IReadOnlyList<RondaResponse> Rondas { get; set; } = [];
}
