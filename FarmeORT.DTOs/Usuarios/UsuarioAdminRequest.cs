using System.ComponentModel.DataAnnotations;

namespace FarmeORT.DTOs.Usuarios;

public sealed class UsuarioAdminRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string Nombre { get; init; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(25, MinimumLength = 3, ErrorMessage = "El apellido debe tener entre 3 y 25 caracteres.")]
    public string Apellido { get; init; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "El nickname es obligatorio.")]
    [RegularExpression("^[a-zA-Z0-9]{3,20}$", ErrorMessage = "El nickname debe ser alfanumérico, sin espacios, de 3 a 20 caracteres.")]
    public string Nickname { get; init; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(25, MinimumLength = 15, ErrorMessage = "La contraseña debe tener entre 15 y 25 caracteres.")]
    [RegularExpression(
        "^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[^a-zA-Z0-9]).*$",
        ErrorMessage = "La contraseña debe tener al menos una mayúscula, una minúscula, un número y un símbolo.")]
    public string Contrasena { get; init; } = string.Empty;

    public DateOnly FechaNacimiento { get; init; }

    [Required(ErrorMessage = "El rol es obligatorio.")]
    [AllowedValues(Roles.Jugador, Roles.Organizador, Roles.Administrador, ErrorMessage = "El rol debe ser Jugador, Organizador o Administrador.")]
    public string Rol { get; init; } = string.Empty;
}
