namespace FarmeORT.IServicios.Sesiones;

public sealed class SesionAutenticada
{
    public Guid Token { get; init; }

    public int UsuarioId { get; init; }

    public string Rol { get; init; } = string.Empty;
}
