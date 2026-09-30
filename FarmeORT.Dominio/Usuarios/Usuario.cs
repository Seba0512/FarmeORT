namespace FarmeORT.Dominio.Usuarios;

public sealed class Usuario
{
    private const int EloInicial = 1200;

    public Usuario(
        string nombre,
        string apellido,
        string email,
        string nickname,
        DateOnly fechaNacimiento,
        Rol rol,
        string contrasenaHash)
    {
        Nombre = nombre;
        Apellido = apellido;
        Email = email;
        Nickname = nickname;
        FechaNacimiento = fechaNacimiento;
        Rol = rol;
        ContrasenaHash = contrasenaHash;
        Elo = EloInicial;
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
