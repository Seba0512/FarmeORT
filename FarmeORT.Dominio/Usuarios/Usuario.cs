namespace FarmeORT.Dominio.Usuarios;

public sealed class Usuario
{
    public int Id { get; init; }

    public Rol Rol { get; init; }

    public string ContrasenaHash { get; init; } = string.Empty;
}
