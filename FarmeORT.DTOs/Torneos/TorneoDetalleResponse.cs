using FarmeORT.DTOs.Rondas;
using FarmeORT.DTOs.Skills;

namespace FarmeORT.DTOs.Torneos;

// El detalle suma al resumen del torneo sus skills obligatorias, los jugadores aceptados y el cuadro (Rondas):
// con una sola llamada alcanza para dibujarlo entero (letra 1.3, D-62).
public sealed class TorneoDetalleResponse : TorneoResponse
{
    public IReadOnlyList<SkillResponse> SkillsObligatorias { get; init; } = [];

    public IReadOnlyList<JugadorAceptadoResponse> JugadoresAceptados { get; init; } = [];

    public IReadOnlyList<RondaResponse> Rondas { get; init; } = [];
}
