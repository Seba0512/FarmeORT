namespace FarmeORT.DTOs.Usuarios;

// Sin validaciones de rango: el servicio usa el tope o el valor por defecto de la paginación,
// "para no trancar al usuario" (AGENTS.md §7).
public sealed class FiltroUsuariosRequest
{
    public string? Nombre { get; init; }

    public string? Apellido { get; init; }

    public string? Nickname { get; init; }

    public int Limit { get; init; } = Paginado.LimitePorDefecto;

    public int Offset { get; init; }
}
