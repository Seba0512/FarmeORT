using FarmeORT.Dominio.Usuarios;

namespace FarmeORT.IRepositorios.Usuarios;

public interface IRepositorioUsuarios
{
    Usuario? ObtenerPorEmail(string email);

    Usuario? ObtenerPorId(int id);

    // Trae el usuario con su repertorio cargado, listo para declarar o quitar skills.
    Usuario? ObtenerConRepertorio(int id);

    IReadOnlyList<Usuario> Listar(FiltroUsuarios filtro);

    bool ExistePorNickname(string nickname);

    void Agregar(Usuario usuario);

    void Actualizar(Usuario usuario);

    void Eliminar(Usuario usuario);

    void GuardarRepertorio(Usuario jugador);

    IReadOnlyList<Usuario> ListarRanking(int cantidad);
}
