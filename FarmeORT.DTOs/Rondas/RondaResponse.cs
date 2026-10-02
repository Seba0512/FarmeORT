namespace FarmeORT.DTOs.Rondas;

public sealed class RondaResponse
{
    public int Numero { get; init; }

    public IReadOnlyList<BatallaDeRondaResponse> Batallas { get; init; } = [];
}
