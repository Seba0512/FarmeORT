namespace FarmeORT.DTOs.Sesiones;

public sealed class LoginRequest
{
    public string Email { get; init; } = string.Empty;

    public string Contrasena { get; init; } = string.Empty;
}
