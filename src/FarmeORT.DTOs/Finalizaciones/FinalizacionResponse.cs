using FarmeORT.DTOs.Batallas;

namespace FarmeORT.DTOs.Finalizaciones;

public sealed class FinalizacionResponse
{
    public int TorneoId { get; init; }

    public string Estado { get; init; } = string.Empty;

    public JugadorBatallaResponse Campeon { get; init; } = new();
}
