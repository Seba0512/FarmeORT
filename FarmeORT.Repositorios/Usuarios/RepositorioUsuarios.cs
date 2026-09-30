using FarmeORT.Dominio.Usuarios;
using FarmeORT.IRepositorios.Usuarios;

namespace FarmeORT.Repositorios.Usuarios;

internal sealed class RepositorioUsuarios(FarmeORTContext contexto) : IRepositorioUsuarios
{
    public Usuario? ObtenerPorEmail(string email)
    {
        return contexto.Usuarios.FirstOrDefault(u => u.Email == email);
    }

    public bool ExistePorNickname(string nickname)
    {
        return contexto.Usuarios.Any(u => u.Nickname == nickname);
    }

    public void Agregar(Usuario usuario)
    {
        throw new NotImplementedException();
    }
}
