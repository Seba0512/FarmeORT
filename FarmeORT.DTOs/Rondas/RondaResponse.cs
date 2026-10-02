using FarmeORT.DTOs.Batallas;

namespace FarmeORT.DTOs.Rondas;

public sealed class RondaResponse
{
    public int Numero { get; init; }

    public IReadOnlyList<BatallaResponse> Batallas { get; init; } = [];
}
