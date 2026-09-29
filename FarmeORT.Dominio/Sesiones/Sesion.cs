using FarmeORT.Dominio.Usuarios;

namespace FarmeORT.Dominio.Sesiones;

public sealed class Sesion(Guid token, Usuario usuario)
{
    public Guid Token { get; } = token;

    public Usuario Usuario { get; } = usuario;
}
