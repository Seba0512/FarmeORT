using FarmeORT.Dominio.Usuarios;

namespace FarmeORT.IRepositorios.Usuarios;

public interface IRepositorioUsuarios
{
    Usuario? ObtenerPorEmail(string email);

    Usuario? ObtenerPorId(int id);

    IReadOnlyList<Usuario> Listar(FiltroUsuarios filtro);

    bool ExistePorNickname(string nickname);

    void Agregar(Usuario usuario);
}
