using FarmeORT.Dominio.Usuarios;

namespace FarmeORT.Dominio.Sesiones;

public sealed class Sesion
{
    public Sesion(Guid token, Usuario usuario)
    {
        Token = token;
        Usuario = usuario;
        UsuarioId = usuario.Id;
    }

    // EF Core materializa la sesión con este constructor y completa la navegación después.
    private Sesion()
    {
        Usuario = null!;
    }

    public Guid Token { get; private set; }

    public int UsuarioId { get; private set; }

    public Usuario Usuario { get; private set; }
}
