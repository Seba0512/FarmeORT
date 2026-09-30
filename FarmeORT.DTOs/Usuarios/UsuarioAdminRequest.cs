namespace FarmeORT.DTOs.Usuarios;

public sealed class UsuarioAdminRequest
{
    public string Nombre { get; init; } = string.Empty;

    public string Apellido { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Nickname { get; init; } = string.Empty;

    public string Contrasena { get; init; } = string.Empty;

    public DateOnly FechaNacimiento { get; init; }

    public string Rol { get; init; } = string.Empty;
}
