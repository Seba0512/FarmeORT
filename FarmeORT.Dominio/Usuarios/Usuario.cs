using System.Net.Mail;
using FarmeORT.Excepciones;

namespace FarmeORT.Dominio.Usuarios;

public sealed class Usuario
{
    private const int EloInicial = 1200;
    private const int LargoMinimoApellido = 3;
    private const int LargoMaximoApellido = 25;

    public Usuario(
        string nombre,
        string apellido,
        string email,
        string nickname,
        DateOnly fechaNacimiento,
        Rol rol,
        string contrasenaHash)
    {
        ValidarNombre(nombre);
        ValidarApellido(apellido);
        ValidarEmail(email);

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

    private static void ValidarNombre(string nombre)
    {
        if(string.IsNullOrWhiteSpace(nombre))
        {
            throw new DatosInvalidosException("El nombre es obligatorio.");
        }
    }

    private static void ValidarApellido(string apellido)
    {
        if(apellido.Length is < LargoMinimoApellido or > LargoMaximoApellido)
        {
            throw new DatosInvalidosException("El apellido debe tener entre 3 y 25 caracteres.");
        }
    }

    private static void ValidarEmail(string email)
    {
        if(!MailAddress.TryCreate(email, out var direccion) || direccion.Address != email)
        {
            throw new DatosInvalidosException("El email no tiene un formato válido.");
        }
    }
}
