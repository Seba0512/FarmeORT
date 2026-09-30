using FarmeORT.DTOs.Usuarios;

namespace FarmeORT.IServicios.Usuarios;

public interface IServicioUsuarios
{
    UsuarioResponse Crear(UsuarioAdminRequest request);

    UsuarioResponse ObtenerPorId(int id);

    IReadOnlyList<UsuarioResponse> Listar(FiltroUsuariosRequest filtro);
}
