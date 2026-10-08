using System.ComponentModel.DataAnnotations;

namespace FarmeORT.DTOs.Sesiones;

public sealed class LoginRequest
{
    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    public string Contrasena { get; init; } = string.Empty;
}
