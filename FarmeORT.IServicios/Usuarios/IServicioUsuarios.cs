using FarmeORT.DTOs.Usuarios;

namespace FarmeORT.IServicios.Usuarios;

public interface IServicioUsuarios
{
    UsuarioResponse RegistrarJugador(RegistroJugadorRequest request);

    UsuarioResponse Crear(UsuarioAdminRequest request);

    UsuarioResponse ObtenerPorId(int id);

    IReadOnlyList<UsuarioResponse> Listar(FiltroUsuariosRequest filtro);

    UsuarioResponse Modificar(int id, UsuarioAdminRequest request, int usuarioActualId);
}
