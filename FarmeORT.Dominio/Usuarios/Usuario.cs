using FarmeORT.Excepciones;

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
        if(string.IsNullOrWhiteSpace(nombre))
        {
            throw new DatosInvalidosException("El nombre es obligatorio.");
        }

        Nombre = nombre;
        Apellido = apellido;
        Email = email;
        Nickname = nickname;
        FechaNacimiento = fechaNacimiento;
        Rol = rol;
        ContrasenaHash = contrasenaHash;
        Elo = EloInicial;
    }

    // EF Core materializa el usuario con este constructor y completa las propiedades después.
    private Usuario()
    {
    }

    public int Id { get; init; }

    public string Nombre { get; private set; } = string.Empty;

    public string Apellido { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string Nickname { get; private set; } = string.Empty;

    public DateOnly FechaNacimiento { get; private set; }

    public Rol Rol { get; private set; }

    public string ContrasenaHash { get; private set; } = string.Empty;

    public int Elo { get; private set; }
}
