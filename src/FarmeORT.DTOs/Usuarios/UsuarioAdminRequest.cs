using System.ComponentModel.DataAnnotations;

namespace FarmeORT.DTOs.Usuarios;

public sealed class UsuarioAdminRequest : RegistroJugadorRequest
{
    [Required(ErrorMessage = "El rol es obligatorio.")]
    [AllowedValues(Roles.Jugador, Roles.Organizador, Roles.Administrador, ErrorMessage = "El rol debe ser Jugador, Organizador o Administrador.")]
    public string Rol { get; init; } = string.Empty;
}
