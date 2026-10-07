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

    // Jugadores sin baja, de mayor a menor ELO. Si empatan va primero el de id menor (el que se registró antes):
    // el desempate es explícito para no depender del orden en que la base devuelve las filas (D-61).
    IReadOnlyList<Usuario> ListarRanking(int cantidad);

    // Igual que ListarRanking, pero solo con los participantes del torneo: inscripción Aceptada o Eliminada (D-45).
    IReadOnlyList<Usuario> ListarRankingDelTorneo(int torneoId, int cantidad);
}
