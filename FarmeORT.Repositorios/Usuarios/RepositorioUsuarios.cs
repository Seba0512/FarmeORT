using FarmeORT.Dominio.Usuarios;
using FarmeORT.IRepositorios.Usuarios;

namespace FarmeORT.Repositorios.Usuarios;

internal sealed class RepositorioUsuarios(FarmeORTContext contexto) : IRepositorioUsuarios
{
    public Usuario? ObtenerPorEmail(string email)
    {
        return contexto.Usuarios.FirstOrDefault(u => u.Email == email);
    }

    public Usuario? ObtenerPorId(int id)
    {
        return contexto.Usuarios.Find(id);
    }

    public IReadOnlyList<Usuario> Listar(FiltroUsuarios filtro)
    {
        return contexto.Usuarios.OrderBy(u => u.Id).ToList();
    }

    public bool ExistePorNickname(string nickname)
    {
        return contexto.Usuarios.Any(u => u.Nickname == nickname);
    }

    public void Agregar(Usuario usuario)
    {
        contexto.Usuarios.Add(usuario);
        contexto.SaveChanges();
    }
}
