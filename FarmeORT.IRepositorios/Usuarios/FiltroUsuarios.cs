namespace FarmeORT.IRepositorios.Usuarios;

public sealed record FiltroUsuarios(string? Nombre, string? Apellido, string? Nickname, int Limite, int Desplazamiento);
