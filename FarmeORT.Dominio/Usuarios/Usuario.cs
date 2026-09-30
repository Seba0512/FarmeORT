namespace FarmeORT.Dominio.Usuarios;

public sealed class Usuario
{
    public int Id { get; init; }

    public string Email { get; init; } = string.Empty;

    public Rol Rol { get; init; }

    public string ContrasenaHash { get; init; } = string.Empty;
}
