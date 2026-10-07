namespace FarmeORT.DTOs.Sesiones;

public sealed class SesionResponse
{
    public string Token { get; init; } = string.Empty;

    public int UsuarioId { get; init; }

    public string Rol { get; init; } = string.Empty;
}
