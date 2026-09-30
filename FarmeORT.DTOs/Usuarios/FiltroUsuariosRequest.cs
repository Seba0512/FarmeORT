namespace FarmeORT.DTOs.Usuarios;

public sealed class FiltroUsuariosRequest
{
    public string? Nombre { get; init; }

    public string? Apellido { get; init; }

    public string? Nickname { get; init; }

    public int Limit { get; init; } = 20;

    public int Offset { get; init; }
}
