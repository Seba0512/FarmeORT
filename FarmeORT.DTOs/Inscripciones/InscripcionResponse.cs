namespace FarmeORT.DTOs.Inscripciones;

public sealed class InscripcionResponse
{
    public int Id { get; init; }

    public int JugadorId { get; init; }

    public string Nickname { get; init; } = string.Empty;

    public int Elo { get; init; }

    public int TorneoId { get; init; }

    public string Torneo { get; init; } = string.Empty;

    public string Estado { get; init; } = string.Empty;

    public DateTime Fecha { get; init; }
}
