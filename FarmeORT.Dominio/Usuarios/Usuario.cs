namespace FarmeORT.Dominio.Usuarios;

public sealed class Usuario
{
    public Usuario(
        string nombre,
        string apellido,
        string email,
        string nickname,
        DateOnly fechaNacimiento,
        Rol rol,
        string contrasenaHash)
    {
        _ = nombre;
        _ = apellido;
        _ = email;
        _ = nickname;
        _ = fechaNacimiento;
        _ = rol;
        _ = contrasenaHash;
        throw new NotImplementedException();
    }

    public Usuario()
    {
    }

    public int Id { get; init; }

    public string Nombre { get; init; } = string.Empty;

    public string Apellido { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Nickname { get; init; } = string.Empty;

    public DateOnly FechaNacimiento { get; init; }

    public Rol Rol { get; init; }

    public string ContrasenaHash { get; init; } = string.Empty;

    public int Elo { get; init; }
}
