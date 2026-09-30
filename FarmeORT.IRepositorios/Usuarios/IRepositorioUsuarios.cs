using FarmeORT.Dominio.Usuarios;

namespace FarmeORT.IRepositorios.Usuarios;

public interface IRepositorioUsuarios
{
    Usuario? ObtenerPorEmail(string email);

    bool ExistePorNickname(string nickname);

    void Agregar(Usuario usuario);
}
